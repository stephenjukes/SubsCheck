using SubsCheck.Models;

namespace SubsCheck.Services;
public interface ISubsWriter
{
    void Write(WriteRequest<IEnumerable<Member>> request);
}
