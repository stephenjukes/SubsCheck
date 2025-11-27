using SubsCheck.Models;
using SubsCheck.Models.Constants.Enums;
using SubsCheck.Models.IO.Input;
using SubsCheck.Services.Interfaces;
using static SubsCheck.Helpers.Helpers;

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
        private readonly List<UnallocatedSub> _unallocated = [];

        private readonly IEnumerable<string> _inputFiles;
        private readonly IEnumerable<string> _transactionFiles;
        private readonly string _outputPath;

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

            var root = ".\\..\\..\\..\\";
            var ioDirectory = Directory.GetDirectories(root, "IO", SearchOption.AllDirectories).FirstOrDefault();

            var inputsDirectory = Directory.GetDirectories(ioDirectory, "Inputs", SearchOption.AllDirectories).FirstOrDefault();
            _inputFiles = Directory.GetFiles(inputsDirectory);

            var transactionsPath = Path.Combine(inputsDirectory, "Transactions");
            _transactionFiles = Directory.GetFiles(transactionsPath);

            _outputPath = Path.Combine(ioDirectory, "Outputs", "Subs.xlsx");
        }

        public async Task<IEnumerable<MemberInput>> CalculateSubs()
        {
            Console.WriteLine("Getting members ...");
            var membersFile = _inputFiles.FirstOrDefault(f => Path.GetFileName(f) == "Members.csv");
            var memberDtos = await _csvDataIO.Read<MemberInput>(new ReadRequest { ResourceLocator = membersFile});
            var families = _memberService.CreateFamilies(memberDtos);

            var allTransactions = new List<TransactionDto>();

            Console.WriteLine($"Processing account {_config.DefaultAccount}");
            var defaultTransactionsFile = _transactionFiles
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == _config.DefaultAccount);
            
            var defaultAccountTransactions = await _csvDataIO.Read<TransactionDto>(
                new ReadRequest { ResourceLocator = defaultTransactionsFile});

            allTransactions.AddRange(defaultAccountTransactions);

            var transactionFiles = _transactionFiles
                .Where(f => Path.GetFileNameWithoutExtension(f) != _config.DefaultAccount);
            
            foreach (var transactionFile in transactionFiles)
            {
                Console.WriteLine($"Processing account {Path.GetFileNameWithoutExtension(transactionFile)}");
                var transactions = await _csvDataIO.Read<TransactionDto>(
                    new ReadRequest { ResourceLocator = transactionFile });

                allTransactions.AddRange(transactions);
            }

            AllocateSubs(allTransactions, families);

            var members = families
                .SelectMany(f => f.Members)
                .OrderBy(m => m.LastName);

            foreach (var member in members)
            {
                AssignConfidenceToSubs(member.Subs);
            }

            Console.WriteLine("Creating output...");
            _subsWriter.Write(new WriteRequest<Member, UnallocatedSub>
            {
                Data = members,
                ResourceLocator = _outputPath,
                Errors = _unallocated
            });

            Console.WriteLine($"File generated. \n\nYou can view the generated file at {Path.GetFullPath(_outputPath)}");

            return null;
        }

        private void AllocateSubs(IEnumerable<TransactionDto> transactions, IEnumerable<Family> families)
        {
            var subs = _subscriptionsService.CreateSubscriptions(transactions, families.ToList());

            var subsByFamily = subs
                .GroupBy(s => s.FamilyAllocation)
                .ToDictionary(g => g.Key, g => g.ToList());

            Console.WriteLine("Processing members...");
            foreach (var family in families)
            {
                var hasSubs = subsByFamily.TryGetValue(family.Id, out List<Subscription> familySubs);
                if (!hasSubs) continue;

                family.Subs = familySubs
                    .OrderBy(s => s.Type)
                    .ThenByDescending(s => int.Parse(s.AccountNumber) == int.Parse(_config.DefaultAccount))
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

            //var paymentCount = (int)(sub.Credit / _config.SubsPrice);

            var selectedSlots = family.Members
                .SelectMany(m => m.Slots, (m, slot) => (Member: m, Slot: slot))
                .Where(x =>
                    isRequiredSlot(x) &&
                    x.Slot.IsAvailable &&
                    x.Slot.Sub is null && 
                    x.Slot.Date <= sub.Date)
                .OrderByDescending(x => x.Member.ReferenceMatchScore)
                .ThenByDescending(x => x.Slot.Date)
                .Take(sub.SubsCount);

            if (selectedSlots.Count() < sub.SubsCount)
            {
                var unallocated = new UnallocatedSub
                {
                    Date = sub.Date,
                    AccountNumber = sub.AccountNumber,
                    Reference = FormatReference(sub.Reference, sub.Credit, sub.Date),
                    TotalSubs = sub.Credit / _config.SubsPrice,
                };

                _unallocated.Add(unallocated);
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
