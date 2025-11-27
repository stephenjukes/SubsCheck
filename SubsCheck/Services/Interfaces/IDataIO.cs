using SubsCheck.Models;

namespace SubsCheck.Services.Interfaces;
public interface IDataIO
{
    Task<IEnumerable<TData>> Read<TData>(ReadRequest request);

    void Write<TData, TError>(WriteRequest<TData, TError> request);
}
