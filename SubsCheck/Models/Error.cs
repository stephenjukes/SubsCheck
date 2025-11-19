namespace SubsCheck.Models;
public class Error
{
    public string Description { get; set; }

    public DateOnly Date { get; set; }

    public string AccountNumber { get; set; }

    public string Family { get; set; }

    public string Reference { get; set; }

    public decimal ReceivedCredit { get; set; }

    public decimal AllocatedCredit { get; set; }

    public decimal TotalSubs { get; set; }

    public decimal AllocatedSubs { get; set; }
}
