namespace SubsCheck.Models
{
    public class Slot
    {
        public DateOnly Date { get; set; }

        public bool IsAvailable { get; set; }

        public Subscription Sub { get; set; }

        public override string ToString()
            => $"{Date}: {Sub?.Credit}";
    }
}
