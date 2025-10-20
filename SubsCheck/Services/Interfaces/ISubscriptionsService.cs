using SubsCheck.Inputs.dto;
using SubsCheck.Models;

namespace SubsCheck.Services.Interfaces;

public interface ISubscriptionsService
{
    IEnumerable<Subscription> CreateSubscriptions(IEnumerable<TransactionDto> transactions, IList<Family> families);

    int AssignReferenceMatchScore(Member member, string reference);
}
