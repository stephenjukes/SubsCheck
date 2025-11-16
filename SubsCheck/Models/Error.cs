namespace SubsCheck.Models;
public class Error
{
    public string Description { get; set; }

    public DateOnly Date { get; set; }

    public decimal Credit { get; set; }

    public string AccountNumber { get; set; }

    public decimal NotAllocated { get; set; }

    public string Reference { get; set; }

    public string Family { get; set; }

    public string Message { get; set; }
}
