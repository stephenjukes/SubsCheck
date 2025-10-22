namespace SubsCheck.Models.Interfaces
{
    public interface IIsAllocatedSubs
    {
        public List<Subscription> Subs { get; set; }

        public int ReferenceMatchScore { get; set; }

        public bool CheckSplitWordsOnly { get; set; }
    }
}
