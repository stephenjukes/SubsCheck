namespace SubsCheck.Models
{
    public class Slot
    {
        public DateOnly Date { get; set; }

        public Transaction Sub { get; set; }

        public override string ToString()
            => $"{Date}: {Sub?.Credit}";
    }
}
