using System.Text.RegularExpressions;
using ClosedXML.Excel;
using SubsCheck.Extensions;
using SubsCheck.Models;
using static SubsCheck.Models.Constants.Enums.AssignmentConfidence;

namespace SubsCheck.Services;
public class SubsWriter : ISubsWriter
{
    private const string Unavailable = "-";
    private const string Unpaid = "x";

    public void Write(WriteRequest<IEnumerable<Member>> request)
    {
        var members = request.Data.ToList();

        using var workbook = new XLWorkbook();

        var detailWorksheet = AddDetailWorksheet1(workbook, members);
        var summaryWorksheet = AddSummaryWorkSheet(workbook, detailWorksheet, members);

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

    private IXLWorksheet AddSummaryWorkSheet(XLWorkbook workbook, IXLWorksheet detailWorksheet, List<Member> members)
    {
        var ws = workbook.AddWorksheet("Summary");
        var range = detailWorksheet.RangeUsed();

        for (int row = 1; row <= range.RowCount(); row++)
        {
            for (int column = 1; column <= range.ColumnCount(); column++)
            {
                var srcAddress = detailWorksheet.Cell(row, column).Address.ToStringRelative();
                var srcValue = $"'{detailWorksheet.Name}'!{srcAddress}";

                ws.Cell(column, row).FormulaA1 = row > 1 && column > 1
                    ? ExtractDate(srcValue)
                    : srcValue;
            }
        }

        ApplySharedFormatting(ws);

        return ws;
    }

    private string ExtractDate(string text)
    {
        var dateLength = 5; // dd/MM
        return $"=RIGHT({text}, {dateLength})";
    }

    private IXLWorksheet AddDetailWorksheet1(XLWorkbook workbook, IEnumerable<Member> members)
    {
        var ws = workbook.AddWorksheet("Detail");

        Action<IXLCell, Slot> formatCell = (cell, slot) =>
        {
            var sub = slot.Sub;

            if (!slot.IsAvailable)
            {
                cell.SetValue(Unavailable);
            }
            else if (sub is null)
            {
                cell.SetValue(Unpaid);
            }
            else
            {
                cell.SetValue($"{sub.Reference} (£{sub.Credit}) {sub.Date:dd/MM}");
                FormatAssignmentConfidence(cell, sub);
            }
        };

        PopulateData(ws, members,
            advanceDate: cell => cell.Row++,
            carriageReturn: cell => { cell.Column++; cell.Row = 1; },
            formatCell: formatCell);

        return ws;
    }

    private IXLWorksheet PopulateData(
        IXLWorksheet ws, 
        IEnumerable<Member> members, 
        Action<Cell> advanceDate,
        Action<Cell> carriageReturn,
        Action<IXLCell, Slot> formatCell)
    {
        // a bit sloppy
        var dateHeaders = GetDateHeaders(members.First());
        var maxScope = Math.Max(dateHeaders.Count() + 1, members.Count() + 1);
        var range = ws.Range(1, 1, maxScope, maxScope);

        ApplySharedFormatting(ws, range);

        var cellPosition = new Cell { Row = 1, Column = 1 };

        foreach (var dateHeader in dateHeaders)
        {
            ws.Cell(cellPosition.Row, cellPosition.Column).Value = dateHeader;
            advanceDate(cellPosition);
        }

        carriageReturn(cellPosition);

        foreach (var member in members)
        {
            var rowHeaders = GetNameHeader(member);

            foreach (var rowHeader in rowHeaders)
            {
                ws.Cell(cellPosition.Row, cellPosition.Column).Value = rowHeader;
                advanceDate(cellPosition);
            }

            foreach (var slot in member.Slots)
            {
                var cell = ws.Cell(cellPosition.Row, cellPosition.Column);
                formatCell(cell, slot);

                advanceDate(cellPosition);
            }

            carriageReturn(cellPosition);
        }

        ws.Columns().AdjustToContents(); // not sure why this isn't retained from ApplySharedFormatting

        return ws;
    }

    private void FormatAssignmentConfidence(IXLCell cell, Subscription sub)
    {
        if (sub.AssignmentConfidence == Medium)
            cell.Style.Font.SetFontColor(XLColor.DarkOrange);

        if (sub.AssignmentConfidence == Low)
            cell.Style.Font.SetFontColor(XLColor.Red);
    }

    private static IEnumerable<string> GetDateHeaders(Member member)
        => GetNameHeader(member).Select(v => "")
            .Concat(member.Slots.Select(s => s.Date.ToString("MMM yy")));

    private static IEnumerable<string> GetNameHeader(Member member)
        => [$"{member.LastName} {member.FirstName}"];

    private static void ApplySharedFormatting(IXLWorksheet ws)
        => ApplySharedFormatting(ws, ws.RangeUsed());

    private static void ApplySharedFormatting(IXLWorksheet ws, IXLRange range)
    {
        if (range is null) return;

        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        
        ws.Columns().AdjustToContents();

        ws.Row(1).Style.Font.SetBold();
        ws.Column(1).Style.Font.SetBold();
        ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
       
        ws.SheetView.Freeze(1, 1);

        range.AddConditionalFormat()
            .WhenEquals(Unpaid)
            .Fill.SetBackgroundColor(XLColor.LightPink)
            .Font.SetFontColor(XLColor.Gray);

        range.AddConditionalFormat()
            .WhenEquals(Unavailable)
            .Fill.SetBackgroundColor(XLColor.LightGray)
            .Font.SetFontColor(XLColor.LightGray);
    }
}
