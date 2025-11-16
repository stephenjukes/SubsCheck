using SubsCheck.Models;
using SubsCheck.Models.IO.Input;

namespace SubsCheck.Services.Interfaces;

public interface ISubscriptionsService
{
    //Task<IEnumerable<TransactionDto>> GetTransactions(ReadRequest request);

    IEnumerable<Subscription> CreateSubscriptions(IEnumerable<TransactionDto> transactions, IList<Family> families);

    int AssignReferenceMatchScore(Member member, string reference);
}
