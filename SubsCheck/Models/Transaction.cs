namespace SubsCheck.Models
{
    public class Transaction
    {
        public DateOnly Date { get; set; }

        public string? Reference { get; set; }

        public decimal Credit { get; set; }

        public override string ToString()
            => $"{Credit}: {Reference}";
    }
}
