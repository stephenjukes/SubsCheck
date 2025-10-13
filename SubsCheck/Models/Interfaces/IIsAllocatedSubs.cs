namespace SubsCheck.Models.Interfaces
{
    public interface IIsAllocatedSubs
    {
        public IEnumerable<Transaction> Subs { get; set; }

        public int ReferenceMatchScore { get; set; }
    }
}
