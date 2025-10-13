using SubsCheck.Inputs.dto.CSV;

namespace SubsCheck.Services
{
    public interface ISubsService
    {
        // This will need to be split out into separate methods
        IEnumerable<CsvMember> CalculateSubs();
    }
}
