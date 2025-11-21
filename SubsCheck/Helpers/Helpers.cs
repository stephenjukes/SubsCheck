namespace SubsCheck.Helpers
{
    public static class Helpers
    {
        public static string FormatReference(string reference, decimal credit, DateOnly date)
        => $"{date:dd/MM/yyyy} (£{credit}) {reference}";
    }
}
