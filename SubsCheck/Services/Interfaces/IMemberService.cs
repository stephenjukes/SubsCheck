using SubsCheck.Models;
using SubsCheck.Models.IO.Input;

namespace SubsCheck.Services.Interfaces;
public interface IMemberService
{
    List<Family> CreateFamilies(IEnumerable<MemberInput> members);
}
