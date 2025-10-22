namespace SubsCheck.Models.IO.Input
{
    public class Configuration
    {
        public decimal SubsPrice { get; set; }

        public DateOnly Start { get; set; }

        public DateOnly End { get; set; }

        public string Culture { get; set; }

        public IEnumerable<string> NonSubsFlags { get; set; }
    }
}
