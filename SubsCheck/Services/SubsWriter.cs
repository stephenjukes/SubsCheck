using ClosedXML.Excel;
using SubsCheck.Extensions;
using SubsCheck.Models;

namespace SubsCheck.Services;
public class SubsWriter : ISubsWriter
{
    private const string Unpaid = "x";

    public void Write(WriteRequest<IEnumerable<Member>> request)
    {
        var members = request.Data;

        using var workbook = new XLWorkbook();
        AddSummaryWorkSheet(workbook, members);
        AddDetailWorkSheet(workbook, members);

        try
        {
            workbook.SaveAs(request.ResourceLocator);
        }
        catch (Exception ex)
        {
            Console.WriteLine("" +
                "An error has occurred, likely because the output document is still open. " +
                "Please ensure the output document is closed and try again.");
        }
    }

    private void AddDetailWorkSheet(XLWorkbook workbook, IEnumerable<Member> members)
    {
        var memberRows = members.Select(m => GetRowHeaders(m).Concat(
            m.Slots.Select(slot => slot.Sub is not null ? slot.Sub.Reference : Unpaid)));

        var rows = GetColumnHeaders(members.First()).Concat(memberRows);

        var memberColumns = Pivot<string>(rows);

        var ws = workbook.AddWorksheet("Detail");
        ws.Cell(1, 1).InsertData(memberColumns);

        var range = ws.RangeUsed();
        if (range is null) return;

        ApplySharedFormatting(ws);
        ws.Columns().AdjustToContents();
    }

    private static void AddSummaryWorkSheet(XLWorkbook workbook, IEnumerable<Member> members)
    {
        var memberRows = members.Select(m => GetRowHeaders(m).Concat(
                m.Slots.Select(slot => slot.Sub is not null ? slot.Sub.Date.ToString("dd/MM") : Unpaid)));

        var rows = GetColumnHeaders(members.First()).Concat(memberRows);

        var ws = workbook.AddWorksheet("Summary");
        ws.Cell(1, 1).InsertData(rows);

        var range = ws.RangeUsed();
        if (range is null) return;

        ApplySharedFormatting(ws);

        // general styling
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var column1 = ws.Column(1);
        column1.AdjustToContents();
        column1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // conditional formatting
        range.AddConditionalFormat()
            .WhenBetween("/", "A") // ie: a number
            .Fill.SetBackgroundColor(XLColor.GrannySmithApple);
    }

    public static IEnumerable<IEnumerable<T>> Pivot<T>(IEnumerable<IEnumerable<T>> data)
    {
        var dataList = data
            .Select(collection => collection.ToList())
            .ToList();

        int rowCount = dataList.Count;
        int colCount = dataList.First().Count;

        var pivoted = new List<List<T>>();

        for (int c = 0; c < colCount; c++)
        {
            var newRow = new List<T>();
            for (int r = 0; r < rowCount; r++)
            {
                newRow.Add(dataList[r][c]);
            }
            pivoted.Add(newRow);
        }
        return pivoted;
    }

    private static IEnumerable<string> GetColumnHeaders(Member member)
        => GetRowHeaders(member).Select(v => "")
            .Concat(member.Slots.Select(s => s.Date.ToString("MMM yy")));

    private static IEnumerable<string> GetRowHeaders(Member member)
        => [$"{member.LastName} {member.FirstName}"];

    private static void ApplySharedFormatting(IXLWorksheet ws)
    {
        var range = ws.RangeUsed();
        if (range is null) return;

        ws.Column(1).Style.Font.SetBold();
        ws.Row(1).Style.Font.SetBold();

        range.Style.Font.FontColor = XLColor.Charcoal;

        // freeze panes
        ws.SheetView.Freeze(1, 1);

        // conditional formatting
        range.AddConditionalFormat() // duplicated
            .WhenEquals(Unpaid)
            .Fill.SetBackgroundColor(XLColor.LightPink)
            .Font.SetFontColor(XLColor.Gray);
    }
}
