using SubsCheck.Models;
using SubsCheck.Models.IO.Input;

namespace SubsCheck.Services.Interfaces;

public interface ISubscriptionsService
{
    IEnumerable<Subscription> CreateSubscriptions(IEnumerable<TransactionDto> transactions, IList<Family> families);

    int AssignReferenceMatchScore(Member member, string reference);
}
