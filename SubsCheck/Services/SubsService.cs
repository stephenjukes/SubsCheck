using SubsCheck.Constants.Enums;
using SubsCheck.Models;
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

            var root = AppContext.BaseDirectory;

            // Data root
            var dataDirectory = Path.Combine(root, "Data");
            if (!Directory.Exists(dataDirectory))
                throw new DirectoryNotFoundException($"Data directory not found: {dataDirectory}");

            // Inputs
            var inputsDirectory = Path.Combine(dataDirectory, "Inputs");
            if (!Directory.Exists(inputsDirectory))
                throw new DirectoryNotFoundException($"Inputs directory not found: {inputsDirectory}");

            _inputFiles = Directory.GetFiles(inputsDirectory, "*.csv");

            // Transactions
            var transactionsDirectory = Path.Combine(inputsDirectory, "Transactions");
            if (!Directory.Exists(transactionsDirectory))
                throw new DirectoryNotFoundException($"Transactions directory not found: {transactionsDirectory}");

            _transactionFiles = Directory.GetFiles(transactionsDirectory, "*.csv");

            // Outputs
            var outputsDirectory = Path.Combine(dataDirectory, "Outputs");
            Directory.CreateDirectory(outputsDirectory); // safe even if it exists

            var datetimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _outputPath = Path.Combine(outputsDirectory, $"Subs_{datetimeString}.xlsx");

        }

        public async Task CalculateSubs()
        {
            var memberDtos = await GetMembers();
            var families = _memberService.CreateFamilies(memberDtos);

            var transactions = await GetTransactions();
            var subs = _subscriptionsService.CreateSubscriptions(transactions, families.ToList());

            foreach (var member in families.SelectMany(f => f.Members))
                member.Slots = _memberService.CreateSlots(subs.First().Date, subs.Last().Date, member);

            AllocateSubs(subs, families);

            var members = PrepareMembersForDisplay(families);
            _unallocated.RemoveAll(u => u.Date < _config.Start || u.Date > _config.End);

            _subsWriter.Write(new WriteRequest<Member, UnallocatedSub>
            {
                Data = members,
                ResourceLocator = _outputPath,
                Errors = _unallocated
            });
        }

        private IEnumerable<Member> PrepareMembersForDisplay(List<Family> families)
        {
            var members = families
                .SelectMany(f => f.Members)
                .OrderBy(m => m.LastName);

            foreach (var member in members)
            {
                AssignConfidenceToSubs(member.Subs);
                member.Slots.RemoveAll(s => s.Date < _config.Start || s.Date > _config.End);
            }

            return members;
        }

        private async Task<IEnumerable<TransactionDto>> GetTransactions()
        {
            var allTransactions = new List<TransactionDto>();
            foreach (var transactionFile in _transactionFiles)
            {
                var transactions = await _csvDataIO.Read<TransactionDto>(
                    new ReadRequest { ResourceLocator = transactionFile });

                allTransactions.AddRange(transactions);
            }

            if (!allTransactions.Any())
                throw new InvalidOperationException("No transactions have been provided.");

            return allTransactions;
        }

        private async Task<IEnumerable<MemberInput>> GetMembers()
        {
            var membersFile = _inputFiles.FirstOrDefault(f => Path.GetFileName(f) == "Members.csv");
            var memberDtos = await _csvDataIO.Read<MemberInput>(new ReadRequest { ResourceLocator = membersFile });

            if (!memberDtos.Any())
                throw new InvalidOperationException("No members have been provided.");

            return memberDtos;
        }

        private void AllocateSubs(IEnumerable<Subscription> subs, IEnumerable<Family> families)
        {
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
                    //.ThenBy(s => s.Date) // removing this seems to help ensure the correct subs are allocated
                    .ThenByDescending(s => s.IsSubScore)
                    .ToList();

                foreach (var sub in family.Subs)
                {
                    // TODO: can we use polymorphism for this?
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
                .GroupBy(s => s.Reference)
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
            Subscription sub, Family family, Func<(Member Member, Slot Slot), bool>? isRequiredSlot = null)
        {
            isRequiredSlot ??= x => true;

            foreach (var member in family.Members)
                member.ReferenceMatchScore = _subscriptionsService.AssignReferenceMatchScore(member, sub.Reference);

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
