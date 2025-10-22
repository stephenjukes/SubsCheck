using SubsCheck.Models.Interfaces;

namespace SubsCheck.Models
{
    public class Family : IIsAllocatedSubs
    {
        public Guid Id { get; set; }

        public Person Mother { get; set; }

        public Person Father { get; set; }

        public IList<Member> Members { get; set; } = [];

        public List<Subscription> Subs { get; set; } = [];

        public int ReferenceMatchScore { get; set; }

        public bool CheckSplitWordsOnly {  get; set; }

        public override string ToString()
            => $"{Mother.LastName} - {Father.LastName}";
    }
}
