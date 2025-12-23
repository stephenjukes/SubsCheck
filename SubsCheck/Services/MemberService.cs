using SubsCheck.Models;
using SubsCheck.Models.IO.Input;
using SubsCheck.Services.Interfaces;

namespace SubsCheck.Services;
public class MemberService : IMemberService
{
    private readonly Configuration _config;
    private readonly IDateService _dateService;

    public MemberService(Configuration config, IDateService dateService)
    {
        _config = config;
        _dateService = dateService;
    }

    public List<Family> CreateFamilies(IEnumerable<MemberInput> members)
    {
        var families = members
            .Where(m =>
                m.Start <= _config.End &&
                (m.End is null || m.End >= _config.Start))
            .GroupBy(m => new { m.MotherLastName, m.MotherFirstName, m.FatherLastName, m.FatherFirstName })
            .Select(family => new Family
            {
                Id = Guid.NewGuid(),
                Mother = new Person { LastName = family.Key.MotherLastName, FirstName = family.Key.MotherFirstName },
                Father = new Person { LastName = family.Key.FatherLastName, FirstName = family.Key.FatherFirstName },
                Members = family.Select(m => new Member
                {
                    LastName = m.LastName,
                    FirstName = m.FirstName,
                    Start = m.Start,
                    End = m.End,
                    CheckSplitWordsOnly = m.CheckSplitWordsOnly ?? false,
                }).ToList(),
                CheckSplitWordsOnly = family.Any(m => m.CheckSplitWordsOnly ?? false)
            })
            .ToList();

        return families;
    }

    public List<Slot> CreateSlots(DateOnly start, DateOnly end, Member member)
    {
        return _dateService.GetMonthRange(start, end)
            .Select(date => new Slot 
                { 
                    Date = new DateOnly(date.Year, date.Month, 1),
                    IsAvailable = date >= member.Start && (member.End is null || date <= member.End),
                })
            .ToList();
    }
}
