using SubsCheck.Models.Interfaces;

namespace SubsCheck.Models
{
    public class Family : IIsAllocatedSubs
    {
        public Person Mother { get; set; }

        public Person Father { get; set; }

        public IEnumerable<Member> Members { get; set; }

        public IEnumerable<Transaction> Subs { get; set; }

        public int ReferenceMatchScore { get; set; }
    }
}
