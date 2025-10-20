using SubsCheck.Inputs.dto;
using SubsCheck.Services;
using System.Text.Json;

namespace SubsCheck
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var basePath = "./../../../";
            var inputs = basePath + "Inputs/";
            var membersFile = inputs + "Members.csv";
            var transactionsFile = inputs + "Transactions.csv";
            var configFile = inputs + "config.json";

            // TODO: Can this be done via Startup?
            var configString = await File.ReadAllTextAsync(configFile);
            var config = JsonSerializer.Deserialize<Configuration>(configString);

            // TODO: DI
            var csvDataIO = new CsvDataIO();
            var excelDataIO = new ExcelDataIO();
            var dateService = new DateService();
            var memberService = new MemberService(config, dateService);
            var subscriptionsService = new SubscriptionsService(config, dateService);

            var subsService = new SubsService(config, csvDataIO, excelDataIO, memberService, subscriptionsService, dateService);

            var subsAllocatedMembers = await subsService.CalculateSubs();
        }
    }
}
