namespace SubsCheck.Models.IO.Output;
public class MemberOutput
{
    public string LastName { get; set; }

    public string FirstName { get; set; }

    public IEnumerable<string> SlotValues { get; set; }
}
