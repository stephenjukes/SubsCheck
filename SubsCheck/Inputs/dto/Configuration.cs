namespace SubsCheck.Inputs.dto
{
    public class Configuration
    {
        public decimal SubsPrice { get; set; }

        public DateOnly Start { get; set; }

        public DateOnly End { get; set; }

        public string Culture { get; set; }
    }
}
