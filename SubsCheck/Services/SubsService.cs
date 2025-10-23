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
        //private readonly IDataIO _excelDataIO;
        private readonly ISubsWriter _subsWriter;
        private readonly List<Error> _errors;
        private static readonly string BaseFile = "./../../../";
        private static readonly string Inputs = BaseFile + "Inputs/";
        private static readonly string Outputs = BaseFile + "Outputs/";
        private static readonly string OutputPath = Outputs + "Subs.xlsx";

        public SubsService(
            Configuration config, 
            IDataIO csvDataIO,
            //IDataIO excelDataIO,
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

            Console.WriteLine("Getting transactions...");
            var transactions = await _csvDataIO.Read<TransactionDto>(new ReadRequest { ResourceLocator = Inputs + "Transactions.csv" });
            var subs = _subscriptionsService.CreateSubscriptions(transactions, families);

            var subsByFamily = subs
                .GroupBy(s => s.FamilyAllocation)
                .ToDictionary(g => g.Key, g => g.ToList());

            var familyCount = families.Count();

            Console.WriteLine("Processing members...");
            foreach (var family in families)
            {
                var hasSubs = subsByFamily.TryGetValue(family.Id, out List<Subscription> familySubs);
                if (!hasSubs) continue;

                family.Subs = familySubs.OrderBy(s => s.Type).ToList();

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

            // TODO: This doesn't belong here.
            // Look at design patterns to see how formatting can be done in conjunction with the excelDataIO
            Console.WriteLine("Creating output...");
            var members = families
                .SelectMany(f => f.Members)
                .OrderBy(m => m.LastName);

            _subsWriter.Write(new WriteRequest<IEnumerable<Member>>
            {
                Data = members,
                ResourceLocator = OutputPath
            });


            //Func<Member, IEnumerable<string>> getRowHeaders = m => [ $"{ m.LastName } { m.FirstName }"];

            //var slotCollections = families
            //    .SelectMany(f => f.Members)
            //    .OrderBy(m => m.LastName)
            //    .Select(m => m.Slots);
            
            //var header = getRowHeaders(new Member()).Select(v => "")
            //    .Concat(members.First().Slots.Select(s => s.Date.ToString("MMM yy")));
            
            //var memberRows = members.Select(m => getRowHeaders(m).Concat(
            //        m.Slots.Select(slot => slot.Sub is not null ? slot.Sub.Date.ToString("dd/MM") : "x")));
            //////////////////////////////////////////////////////////////////////////////////////////////////

            //_excelDataIO.Write(new WriteRequest<IEnumerable<IEnumerable<Slot>>>
            //{
            //    // create an extenstion to make this nicer
            //    Data = slotCollections,
            //    ResourceLocator = OutputPath
            //});

            Console.WriteLine($"File generated. \n\nYou can view the generated file at {Path.GetFullPath(OutputPath)}");

            return null;
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
                .Where(x => x.Slot.Sub is null && x.Slot.Date <= sub.Date)
                .OrderByDescending(x => x.Member.ReferenceMatchScore)
                .ThenByDescending(x => x.Slot.Date)
                .Where(isRequiredSlot)
                .Take(paymentCount);
            // not reversing for now unless we see a need

            //if (selectedSlots.Count() < paymentCount)
            //    _errors.Add(new Error
            //    {
            //        Description = "Unable to allocate full subscription",
            //        Message = $"Sub for £{sub.Credit} " +
            //            $"with reference {sub.Reference} could not be allocated to the " +
            //            $"{selectedSlots.Count()} slots found for the " +
            //            $"{family.Father.LastName} family"
            //    });

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
