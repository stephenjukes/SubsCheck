using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubsCheck.Inputs.dto.CSV;
using SubsCheck.Models;

namespace SubsCheck
{
    public static class Mappers
    {
        public static Member ToMember(CsvMember csvMember)
        {
            return new Member
            {
                FirstName = csvMember.FirstName,
                LastName = csvMember.LastName,
                Start = csvMember.Start,
                End = csvMember.End,
                ReferenceMatchScore = 0,
                CheckSplitWordOnly = csvMember.CheckSplitWordsOnly ?? false,
                Subs = [],
                Slots = [] // TODO: Populate
            };
        }

        public static Transaction ToTransaction(CsvTransaction csvTransaction)
        {
            return new Transaction
            {
                Date = csvTransaction.Date,
                Credit = csvTransaction.Credit ?? 0,
                Reference = csvTransaction.Reference
            };
        }
    }
}
