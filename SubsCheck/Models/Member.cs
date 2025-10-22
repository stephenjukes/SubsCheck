using SubsCheck.Models.Interfaces;

namespace SubsCheck.Models
{
    public class Member : Person, IIsAllocatedSubs
    {
        public DateOnly Start { get; set; }

        public DateOnly? End { get; set; }

        public bool CheckSplitWordsOnly { get; set; }

        public IEnumerable<Slot> Slots { get; set; } = [];

        public List<Subscription> Subs { get; set; } = [];

        public int ReferenceMatchScore { get; set; }
    }
}
