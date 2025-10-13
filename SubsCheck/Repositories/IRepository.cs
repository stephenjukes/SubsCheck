using SubsCheck.Services.dto;

namespace SubsCheck.Repositories
{
    public interface IRepository
    {
        Task<IEnumerable<T>> GetAll<T>(GetRequest request);
    }
}
