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

    public IList<Family> CreateFamilies(IEnumerable<MemberInput> members)
    {
        var slots = _dateService.GetMonthRange(_config.Start, _config.End)
            .Select(date => new Slot { Date = date })
            .ToList();

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
                    Slots = CreateSlots(_config.Start, _config.End)
                }).ToList(),
                CheckSplitWordsOnly = family.Any(m => m.CheckSplitWordsOnly ?? false)
            })
            .ToList();

        return families;
    }

    private List<Slot> CreateSlots(DateOnly start, DateOnly end)
    {
        return _dateService.GetMonthRange(start, end)
            .Select(date => new Slot { Date = date })
            .ToList();
    }
}
