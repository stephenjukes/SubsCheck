using CsvHelper.Configuration.Attributes;

namespace SubsCheck.Models.IO.Input
{
    public class TransactionDto
    {
        [Name("Transaction Date")]
        public DateOnly Date { get; set; }

        [Name("Account Number")]
        public string AccountNumber { get; set; }

        [Name("Transaction Description")]
        public string Reference { get; set; }

        [Name("Credit Amount")]
        public decimal? Credit { get; set; }

        public override string ToString()
            => $"{Credit}: {Reference}";
    }
}
