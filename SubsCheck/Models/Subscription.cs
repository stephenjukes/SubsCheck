using SubsCheck.Models.Constants.Enums;

namespace SubsCheck.Models;
public class Subscription : Transaction
{
    public int IsSubScore { get; set; }

    public Guid FamilyAllocation { get; set; }

    public SubscriptionType Type { get; set; }

    // public Func<Slot, Slot> GetSlots { get; set; }
}
