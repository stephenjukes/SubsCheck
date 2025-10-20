using SubsCheck.Extensions;
using SubsCheck.Inputs.dto;
using SubsCheck.Models;
using SubsCheck.Models.Constants.Enums;
using SubsCheck.Services.Interfaces;

namespace SubsCheck.Services;
public class SubscriptionsService : ISubscriptionsService
{
    private readonly Configuration _config;
    private readonly IDateService _dateService;
    private const int SubsScoreThreshold = 25;

    public SubscriptionsService(Configuration config, IDateService dateService)
    {
        _config = config;
        _dateService = dateService;
    }

    public IEnumerable<Subscription> CreateSubscriptions(
        IEnumerable<TransactionDto> transactions, IList<Family> families)
    {
        var subs = transactions
            .Where(t =>
                t.Date >= _config.Start &&
                t.Date <= _config.End &&
                t.Credit is not null &&
                t.Credit % _config.SubsPrice == 0 &&
                _config.NonSubsFlags.All(flag => !t.Reference.Contains(flag)))
            .Select(ToSubscription)
            .ToList()
            ;

        foreach (var sub in subs)
        {
            // TODO: Refactor subs score to its own method
            var reference = sub.Reference;

            if (reference is null)
                continue;

            var matchedFamily = MatchFamilyToReference(sub.Reference!, families);

            var score = 0;
            score += matchedFamily.ReferenceMatchScore;
            if (reference.Split(" ").Contains("subs")) score += 30;

            sub.IsSubScore = score;
            sub.FamilyAllocation = matchedFamily.Id;

            sub.Type = _dateService.GetMonthsFromText(reference).Any()
                ? SubscriptionType.Backdated
                : SubscriptionType.Regular;
        }

        return subs
            .Where(s => s.IsSubScore >= SubsScoreThreshold)
            .OrderBy(s => s.Date);
    }

    private  static Subscription ToSubscription(TransactionDto csvTransaction)
    {
        return new Subscription
        {
            Date = csvTransaction.Date,
            Credit = csvTransaction.Credit ?? 0,
            Reference = (csvTransaction.Reference ?? "").ToLower(),
        };
    }

    // TODO: Should this have its own service?
    private Family MatchFamilyToReference(string reference, IList<Family> families)
    {
        //var familyList = families.ToList();
        foreach (var f in families)
        {
            var score = 0;

            if (f.Members.Any(m => reference.ContainsIsolatedText(m.LastName.ToLower())))                       score += 30;
            if (!f.CheckSplitWordsOnly && f.Members.Any(m => reference.Contains(m.LastName.ToLower())))         score += 25;
            if (f.Members.Any(m => reference.ContainsIsolatedText(m.FirstName.ToLower())))                      score += 20;
            if (!f.CheckSplitWordsOnly && f.Members.Any(m => reference.Contains(m.FirstName.ToLower())))        score += 20;
            if (f.Members.Any(m => reference.ContainsIsolatedText(m.FirstName.First().ToString().ToLower())))   score += 10;

            if (reference.ContainsIsolatedText(f.Mother.LastName.ToLower()))                                    score += 20;
            if (!f.CheckSplitWordsOnly && reference.Contains(f.Mother.LastName.ToLower()))                      score += 15;
            if (f.Members.Any(m => reference.ContainsIsolatedText(m.FirstName.First().ToString().ToLower())))   score += 5;
            if (reference.ContainsIsolatedText(f.Mother.FirstName.ToLower()))                                   score += 1;
            // full firstnames are more likely to be for members than parents

            if (reference.ContainsIsolatedText(f.Father.LastName.ToLower()))                                    score += 20;
            if (!f.CheckSplitWordsOnly && reference.Contains(f.Father.LastName.ToLower()))                      score += 15;
            if (f.Members.Any(m => reference.ContainsIsolatedText(m.FirstName.First().ToString().ToLower())))   score += 5;
            if (reference.ContainsIsolatedText(f.Father.FirstName.ToLower()))                                   score += 1;
            // full firstnames are more likely to be for members than parents

            f.ReferenceMatchScore = score;
        }

        var rankedFamilies = families.OrderByDescending(f => f.ReferenceMatchScore);
        return rankedFamilies.First();
    }

    public int AssignReferenceMatchScore(Member member, string reference)
    {
        var score = 0;

        // These are within the same family anyway, so maybe first names should take precendence?
        //if (reference.ContainsIsolatedText(member.LastName.ToLower()))            score += 30;
        //if (reference.Contains(member.LastName.ToLower()))                        score += 25;
        if (reference.ContainsIsolatedText(member.FirstName.ToLower()))             score += 20;
        if (reference.Contains(member.FirstName.ToLower()))                         score += 15;
        if (reference.ContainsIsolatedText(member.FirstName.First().ToString()))    score += 10;

        return score;
    }
}
