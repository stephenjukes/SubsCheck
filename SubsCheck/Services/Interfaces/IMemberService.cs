using SubsCheck.Inputs.dto;
using SubsCheck.Models;

namespace SubsCheck.Services.Interfaces;
public interface IMemberService
{
    IList<Family> CreateFamilies(IEnumerable<MemberDto> members);
}
