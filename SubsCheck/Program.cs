using SubsCheck.Models.IO.Input;
using SubsCheck.Services;
using System.Text.Json;

// TODO:
// DI
// Put inputs out outputs folders into an IO folder
// Try to get an absolute path to the inputs and outputs folder
// Take maximum transaction range, (at least 6 months before and after desired range)
// Change Error class to Unallocated
// Clarify in excel which columns are readonly and which can be updated
// Add in the beaver start and end dates if available

// Unallocated tab
    // Decide on another first column in Unallocated
    // Centralise text in the 4 added columns
    // Format other accounts in blue
    // add a comment to prompt notes depending on the outcome

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

            // TODO: DI
            // TODO: Can this be done via Startup?
            var configString = await File.ReadAllTextAsync(configFile);
            var config = JsonSerializer.Deserialize<Configuration>(configString);

            // TODO: DI
            var csvDataIO = new CsvDataIO();
            var subsWriter = new SubsWriter(config);
            var dateService = new DateService();
            var memberService = new MemberService(config, dateService);
            var subscriptionsService = new SubscriptionsService(config, dateService);

            var subsService = new SubsService(config, csvDataIO, subsWriter, memberService, subscriptionsService, dateService);

            var subsAllocatedMembers = await subsService.CalculateSubs();
        }
    }
}
