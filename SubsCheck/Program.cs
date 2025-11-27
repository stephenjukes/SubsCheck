using SubsCheck.Models.IO.Input;
using SubsCheck.Services;
using System.Text.Json;

// TODO:

// DONE Put inputs out outputs folders into an IO folder
// DONE Try to get an absolute path to the inputs and outputs folder
//      Take maximum transaction range, (at least 6 months before and after desired range)
// DONE Change Error class to Unallocated
// DONE Clarify in excel which columns are readonly and which can be updated
//      Add in the beaver start and end dates if available
//      Deal with warnings (deference of a possibly null reference)

// Unallocated tab
// DONE Decide on another first column in Unallocated
// DONE Centralise text in the 4 added columns
// DONE centralise row start variable
// INGORE add a comment to prompt notes depending on the outcome

// README
// DI

namespace SubsCheck
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var root = ".\\..\\..\\..\\";
            var inputs = Directory
                .GetDirectories(root, "Inputs", SearchOption.AllDirectories)
                .FirstOrDefault();

            var configFile = Directory
                .GetFiles(inputs, "config.json", SearchOption.AllDirectories)
                .FirstOrDefault();
               
            var configString = await File.ReadAllTextAsync(configFile);
            var config = JsonSerializer.Deserialize<Configuration>(configString);

            var csvDataIO = new CsvDataIO();
            var subsWriter = new SubsWriter(config);
            var dateService = new DateService();
            var memberService = new MemberService(config, dateService);
            var subscriptionsService = new SubscriptionsService(config, dateService);

            var subsService = new SubsService(config, csvDataIO, subsWriter, memberService, subscriptionsService, dateService);
            _ = await subsService.CalculateSubs();
        }
    }
}
