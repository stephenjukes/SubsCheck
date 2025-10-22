using SubsCheck.Models;
using SubsCheck.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SubsCheck.Services;
public class DateService : IDateService
{
    public IEnumerable<Month> Months { get; } = Enumerable.Range(1, 12)
        .Select(i => new Month
        {
            Number = i,
            Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)
        });

    public IEnumerable<Month> MonthsFullAndAbbreviated => Months
        .Concat(AbbreviatedMonths)
        .Concat(
            [
                new Month
                {   // the only other abbreviation I can imagine being used
                    Name = "Sept",
                    Number = 9
                }
            ]);

    public IEnumerable<Month> GetMonthsFromText(string text)
       => MonthsFullAndAbbreviated.Where(m => Regex.Split(text, @"\W").Contains(m.Name.ToLower()));

    public IEnumerable<DateOnly> GetMonthRange(DateOnly start, DateOnly end)
    {
        var months = new List<DateOnly>();
        var current = start;

        while (current <= end)
        {
            months.Add(current);
            current = current.AddMonths(1);
        }

        return months;
    }

    private IEnumerable<Month> AbbreviatedMonths {get;} = Enumerable.Range(1, 12)
        .Select(i => new Month
        {
            Number = i,
            Name = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i)
        });
}
