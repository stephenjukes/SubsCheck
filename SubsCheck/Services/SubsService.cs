using System.Text.RegularExpressions;
using System.Transactions;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using SubsCheck.Models;
using SubsCheck.Models.Constants.Enums;
using SubsCheck.Models.IO.Input;
using SubsCheck.Models.IO.Output;
using SubsCheck.Services.Interfaces;

namespace SubsCheck.Services
{
    public class SubsService : ISubsService
    {
        private readonly Configuration _config;
        private readonly IMemberService _memberService;
        private readonly ISubscriptionsService _subscriptionsService;
        private readonly IDateService _dateService;
        private readonly IDataIO _csvDataIO;
        private readonly ISubsWriter _subsWriter;
        private readonly List<Error> _errors = [];

        private static readonly string BaseFile = "./../../../";
        private static readonly string Inputs = BaseFile + "Inputs/";
        private static readonly string TransactionsDirectory = Inputs + "Transactions/";
        private static readonly string Outputs = BaseFile + "Outputs/";

        private static readonly string IsDefaultAccount = "isDefaultAccount";
        private static readonly string IsNotDefaultAccount = "isNotDefaultAccount";

        private static readonly string OutputPath = Outputs + "Subs.xlsx";

        public SubsService(
            Configuration config, 
            IDataIO csvDataIO,
            ISubsWriter subsWriter,
            IMemberService memberService, 
            ISubscriptionsService subscriptionsService, 
            IDateService dateService)
        {
            _config = config;
            _csvDataIO = csvDataIO;
            _subsWriter = subsWriter;
            _memberService = memberService;
            _subscriptionsService = subscriptionsService;
            _dateService = dateService;
        }

        public async Task<IEnumerable<MemberInput>> CalculateSubs()
        {
            Console.WriteLine("Processing started ...");
            var errors = new List<Error>();

            Console.WriteLine("Getting members ...");
            var memberDtos = await _csvDataIO.Read<MemberInput>(new ReadRequest { ResourceLocator = Inputs + "Members.csv" });
            var families = _memberService.CreateFamilies(memberDtos);

            Console.WriteLine($"Processing account {_config.DefaultAccount}");
            var defaultAccountTransactions = await _csvDataIO.Read<TransactionDto>(
                new ReadRequest { ResourceLocator = TransactionsDirectory + _config.DefaultAccount + ".csv" });

            AllocateSubs(defaultAccountTransactions, families);

            var transactionFiles = Directory.GetFiles(TransactionsDirectory)
                .Where(filename => Path.GetFileNameWithoutExtension(filename) != _config.DefaultAccount);
            
            foreach (var transactionFile in transactionFiles)
            {
                Console.WriteLine($"Processing account {Path.GetFileNameWithoutExtension(transactionFile)}");
                var transactions = await _csvDataIO.Read<TransactionDto>(
                    new ReadRequest { ResourceLocator = transactionFile });

                AllocateSubs(transactions, families);
            }

            var members = families
                .SelectMany(f => f.Members)
                .OrderBy(m => m.LastName);

            foreach (var member in members)
            {
                AssignConfidenceToSubs(member.Subs);
            }

            Console.WriteLine("Creating output...");
            _subsWriter.Write(new WriteRequest<IEnumerable<Member>>
            {
                Data = members,
                ResourceLocator = OutputPath,
                Errors = _errors
            });


            Console.WriteLine($"File generated. \n\nYou can view the generated file at {Path.GetFullPath(OutputPath)}");

            return null;
        }

        private void AllocateSubs(IEnumerable<TransactionDto> transactions, IEnumerable<Family> families)
        {
            var subs = _subscriptionsService.CreateSubscriptions(transactions, families.ToList());

            var subsByFamily = subs
                .GroupBy(s => s.FamilyAllocation)
                .ToDictionary(g => g.Key, g => g.ToList());

            // var familyCount = families.Count();

            Console.WriteLine("Processing members...");
            foreach (var family in families)
            {
                var hasSubs = subsByFamily.TryGetValue(family.Id, out List<Subscription> familySubs);
                if (!hasSubs) continue;

                family.Subs = familySubs
                    .OrderBy(s => s.Type)
                    .ThenByDescending(s => s.IsSubScore)
                    .ToList();

                foreach (var sub in family.Subs)
                {
                    // TODO: is there a better way to do this?
                    switch (sub.Type)
                    {
                        case SubscriptionType.Backdated:
                            AllocateSubToMember(sub, family, IsBackdatedSlot(sub));
                            break;
                        case SubscriptionType.Regular:
                            AllocateSubToMember(sub, family);
                            break;
                        default: throw new InvalidOperationException($"Subscription type '{sub.Type}' is not recognised");
                    }
                }
            }
        }

        // TODO: Should this go in the subscriptions service?
        private static void AssignConfidenceToSubs(List<Subscription> subs)
        {
            if (!subs.Any()) return;

            var referenceGroups = subs
                .GroupBy(s => s.Reference) // Regex.Match(s.Reference, @"(.*?)\s*(?=\s+\S*\d{3}|$)").Groups[1].Value)
                .OrderByDescending(group => group.Count());

            var modelReference = referenceGroups.First();

            foreach (var referenceGroup in referenceGroups)
            {
                if (referenceGroup.Count() == 1)
                    AssignConfidenceToSubs(AssignmentConfidence.Low, referenceGroup);
                else if (referenceGroup.Count() == modelReference.Count())
                    AssignConfidenceToSubs(AssignmentConfidence.High, referenceGroup);
                else
                    AssignConfidenceToSubs(AssignmentConfidence.Medium, referenceGroup);
            }
        }

        private static void AssignConfidenceToSubs(AssignmentConfidence assignment, IEnumerable<Subscription> subs)
        {
            foreach (var sub in subs)
                sub.AssignmentConfidence = assignment;
        }

        private void AllocateSubToMember(
            Subscription sub, 
            Family family, 
            Func<(Member Member, Slot Slot), bool>? isRequiredSlot = null)
        {
            isRequiredSlot ??= x => true;

            foreach (var member in family.Members)
                member.ReferenceMatchScore = _subscriptionsService.AssignReferenceMatchScore(member, sub.Reference);

            var paymentCount = (int)(sub.Credit / _config.SubsPrice);

            var selectedSlots = family.Members
                .SelectMany(m => m.Slots, (m, slot) => (Member: m, Slot: slot))
                .Where(x =>
                    isRequiredSlot(x) &&
                    x.Slot.IsAvailable &&
                    x.Slot.Sub is null && 
                    x.Slot.Date <= sub.Date)
                .OrderByDescending(x => x.Member.ReferenceMatchScore)
                .ThenByDescending(x => x.Slot.Date)
                .Take(paymentCount);

            if (selectedSlots.Count() < paymentCount)
            {
                var error = new Error
                {
                    Description = "Unable to allocate",
                    Date = sub.Date,
                    Credit = sub.Credit,
                    AccountNumber = sub.AccountNumber,
                    NotAllocated = sub.Credit - (selectedSlots.Count() * _config.SubsPrice),
                    Reference = sub.Reference,
                    Family = family.Father.LastName
                };

                _errors.Add(error);
            }

            foreach (var (member, slot) in selectedSlots)
            {
                slot.Sub = sub;
                member.Subs.Add(sub);
            }
        }

        // TODO: Move to DateService or even a BackdatedAllocation service?
        /////////////////////////////////////////////////////////////////////////////
        private Func<(Member Member, Slot Slot), bool> IsBackdatedSlot(Subscription sub)
        {
            var monthsFromReference = _dateService.GetMonthsFromText(sub.Reference);
            var backdatedMonths = GetBackdatedAllocatedMonths(monthsFromReference, sub.Date);

            return x => backdatedMonths.Contains(x.Slot.Date);
        }

        private IEnumerable<DateOnly> GetBackdatedAllocatedMonths(IEnumerable<Month> monthsInReference, DateOnly paymentDate)
        {
            if (monthsInReference.Count() != 2)
                return monthsInReference.Select(m => MonthToDate(m, paymentDate));

            // to dates assumes a range
            var startDate = MonthToDate(monthsInReference.First(), paymentDate);
            var endDate = MonthToDate(monthsInReference.Last(), paymentDate);

            return _dateService.GetMonthRange(startDate, endDate);
        }

        private DateOnly MonthToDate(Month designatedMonth, DateOnly paymentDate)
        {
            var allocationYear = designatedMonth.Number <= paymentDate.Month
                ? paymentDate.Year
                : paymentDate.Year - 1;

            return new DateOnly(allocationYear, designatedMonth.Number, 1);
        }
        /////////////////////////////////////////////////////////////////////////////
    }
}
