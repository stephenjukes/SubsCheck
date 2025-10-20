using SubsCheck.Models;

namespace SubsCheck.Services;
public class CalculateSubsResponse
{
    public IEnumerable<Member> Members { get; set; }

    public IEnumerable<Error>  Errors { get; set; }
}
