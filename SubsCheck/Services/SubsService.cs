using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SubsCheck.Inputs.dto.CSV;
using SubsCheck.Models;
using SubsCheck.Repositories;
using static SubsCheck.Mappers;

namespace SubsCheck.Services
{
    public class SubsService : ISubsService
    {
        public IEnumerable<CsvMember> CalculateSubs()
        {
            //// TODO: Can this be done via Startup?
            //var configString = await File.ReadAllTextAsync(ConfigFile);
            //var config = JsonSerializer.Deserialize<Configuration>(configString);

            //var csvMembers = await _repository
            //    .GetAll<CsvMember>(new GetRequest { Path = MembersFile });

            //// Actually, we want to create members within families immediately?
            //var members = csvMembers
            //    .Where(m =>
            //        m.Start <= config.End &&
            //        (m.End is null || m.End >= config.Start))
            //    .Select(ToMember);

            //var csvTransactions = await _repository.GetAll<CsvTransaction>(
            //    new GetRequest { Path = TransactionsFile });

            //var transactions = csvTransactions
            //    .Where(t => 
            //        t.Credit is not null && 
            //        t.Date >= config.Start &&
            //        t.Date <= config.End)
            //    .Select(ToTransaction);


            return null;
        }
    }
}
