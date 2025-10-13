using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using SubsCheck.Services.dto;

namespace SubsCheck.Repositories
{
    public class CsvRepository : IRepository
    {
        public async Task<IEnumerable<T>> GetAll<T>(GetRequest request)
        {
            // this should really go in config
            var gbConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("en-GB"));

            using var reader = new StreamReader(request.Path);
            using var csv = new CsvReader(reader, gbConfig);

            return await csv.GetRecordsAsync<T>().ToListAsync();
        }
    }
}
