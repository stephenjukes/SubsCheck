using SubsCheck.Models;
using SubsCheck.Models.IO.Input;

namespace SubsCheck
{
    public static class Mappers
    {
        public static Member ToMember(MemberInput csvMember)
        {
            return new Member
            {
                FirstName = csvMember.FirstName,
                LastName = csvMember.LastName,
                Start = csvMember.Start,
                End = csvMember.End,
                ReferenceMatchScore = 0,
                CheckSplitWordsOnly = csvMember.CheckSplitWordsOnly ?? false,
                Subs = [],
                Slots = [] // TODO: Populate
            };
        }

        public static Transaction ToTransaction(TransactionDto csvTransaction)
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
