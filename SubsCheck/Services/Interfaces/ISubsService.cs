using SubsCheck.Inputs.dto;

namespace SubsCheck.Services.Interfaces
{
    public interface ISubsService
    {
        // This will need to be split out into separate methods
        Task<IEnumerable<MemberDto>> CalculateSubs();
    }
}
