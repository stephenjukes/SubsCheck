using SubsCheck.Models.IO.Input;

namespace SubsCheck.Services.Interfaces
{
    public interface ISubsService
    {
        // This will need to be split out into separate methods
        Task<IEnumerable<MemberInput>> CalculateSubs();
    }
}
