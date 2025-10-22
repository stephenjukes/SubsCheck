using CsvHelper;
using CsvHelper.Configuration;
using SubsCheck.Models;
using SubsCheck.Services.Interfaces;
using System.Globalization;

namespace SubsCheck.Services;
public class CsvDataIO : IDataIO
{
    public async Task<IEnumerable<T>> Read<T>(ReadRequest request)
    {
        // this should really go in config
        var gbConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("en-GB"));

        using var reader = new StreamReader(request.ResourceLocator);
        using var csv = new CsvReader(reader, gbConfig);
        var enitities = await csv.GetRecordsAsync<T>().ToListAsync();

        return enitities;
    }

    public void Write<T>(WriteRequest<T> request)
    {
        throw new NotImplementedException();
    }
}
