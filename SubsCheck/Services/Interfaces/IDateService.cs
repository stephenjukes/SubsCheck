using SubsCheck.Models;

namespace SubsCheck.Services.Interfaces;
public interface IDateService
{
    IEnumerable<Month> Months { get; }

    IEnumerable<Month> MonthsFullAndAbbreviated { get; }

    IEnumerable<DateOnly> GetMonthRange(DateOnly start, DateOnly end);

    IEnumerable<Month> GetMonthsFromText(string text);
}
