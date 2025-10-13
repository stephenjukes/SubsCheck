using CsvHelper.Configuration.Attributes;

namespace SubsCheck.Inputs.dto.CSV
{
    public class CsvTransaction
    {
        [Name("Transaction Date")]
        public DateOnly Date { get; set; }

        [Name("Transaction Type")]
        public string Type { get; set; }

        [Name("Transaction Description")]
        public string Reference { get; set; }

        [Name("Credit Amount")]
        public decimal? Credit { get; set; }
    }
}
