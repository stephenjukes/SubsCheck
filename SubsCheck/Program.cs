using System.Text.Json;
using SubsCheck.Inputs.dto;
using SubsCheck.Inputs.dto.CSV;
using SubsCheck.Repositories;
using SubsCheck.Services;
using static SubsCheck.Mappers;

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
            var csvRepository = new CsvRepository();
            var subsService = new SubsService();

            // TODO: Can this be done via Startup?
            var configString = await File.ReadAllTextAsync(configFile);
            var config = JsonSerializer.Deserialize<Configuration>(configString);

            var csvMembers = await csvRepository
                .GetAll<CsvMember>(new GetRequest { Path = membersFile });

            // Actually, we want to create members within families immediately?
            var members = csvMembers
                .Where(m =>
                    m.Start <= config.End &&
                (m.End is null || m.End >= config.Start))
                .Select(ToMember);

            var csvTransactions = await csvRepository.GetAll<CsvTransaction>(
                new GetRequest { Path = transactionsFile });

            var transactions = csvTransactions
                .Where(t =>
                    t.Credit is not null &&
                    t.Date >= config.Start &&
                    t.Date <= config.End)
                .Select(ToTransaction);




            var subsAllocatedMembers = subsService.CalculateSubs();


            Console.WriteLine(args.Length);

            foreach (string arg in args)
            {
                
            }

            //Console.WriteLine(args[0]);

            //var configuration = JsonSerializer.Deserialize<Configuration>(args.First());

            //var properties = typeof(Configuration).GetProperties();
            //foreach (var property in properties)
            //{
            //    Console.WriteLine($"{property.Name}: {property.GetValue(configuration)}");
            //}
        }
    }
}
