using SubsCheck.Models;

namespace SubsCheck.Services.Interfaces;
public interface ISubsWriter
{
    void Write(WriteRequest<Member, UnallocatedSub> request);
}
