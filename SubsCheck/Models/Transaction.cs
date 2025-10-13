namespace SubsCheck.Models
{
    public class Transaction
    {
        public DateOnly Date { get; set; }

        public string? Reference { get; set; }

        public decimal Credit { get; set; }

        public int IsSubScore { get; set; }

        // public Func< MyProperty { get; set; }
    }
}
