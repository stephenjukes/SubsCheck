namespace SubsCheck.Models;
public class UnallocatedSub
{
    public DateOnly Date { get; set; }

    public string AccountNumber { get; set; }

    public string Reference { get; set; }

    public decimal TotalSubs { get; set; }
}
