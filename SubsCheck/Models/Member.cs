using SubsCheck.Models.Interfaces;

namespace SubsCheck.Models
{
    public class Member : Person, IIsAllocatedSubs
    {
        public DateOnly Start { get; set; }

        public DateOnly? End { get; set; }

        public bool CheckSplitWordOnly { get; set; }

        public IEnumerable<Slot> Slots { get; set; } = [];

        public IEnumerable<Transaction> Subs { get; set; } = [];

        public int ReferenceMatchScore { get; set; }
    }
}
