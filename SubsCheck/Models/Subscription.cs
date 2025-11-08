using SubsCheck.Models.Constants.Enums;

namespace SubsCheck.Models;
public class Subscription : Transaction
{
    public int IsSubScore { get; set; }

    public Guid FamilyAllocation { get; set; }

    public AssignmentConfidence AssignmentConfidence { get; set; }

    public SubscriptionType Type { get; set; }
}
