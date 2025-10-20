using SubsCheck.Models;

namespace SubsCheck.Services.Interfaces;
public interface IDataIO
{
    Task<IEnumerable<T>> Read<T>(ReadRequest request);

    void Write<T>(WriteRequest<T> request);
}
