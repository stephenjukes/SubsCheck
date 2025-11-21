using ClosedXML.Excel;
using SubsCheck.Constants.Enums;
using SubsCheck.Extensions;
using SubsCheck.Extensions.Excel;
using SubsCheck.Models;
using SubsCheck.Models.Constants.Enums;
using SubsCheck.Models.Excel;
using SubsCheck.Models.IO.Input;
using static SubsCheck.Constants.Excel;
using static SubsCheck.Helpers.Helpers;

namespace SubsCheck.Services;
public class SubsWriter : ISubsWriter
{
    private readonly Configuration _config;
    private const string Unavailable = "-";
    private const string Unpaid = "x";

    public SubsWriter(Configuration config)
    {
        _config = config;
    }

    public void Write(WriteRequest<IEnumerable<Member>> request)
    {
        var members = request.Data.ToList();

        using var workbook = new XLWorkbook();

        Console.WriteLine($"Creating {WorksheetNames.Detail} worksheet...");
        AddDetailWorksheet(workbook, members);

        Console.WriteLine($"Creating {WorksheetNames.Unallocated} worksheet...");
        AddUnallocatedWorksheet(workbook, request.Errors);

        Console.WriteLine($"Creating {WorksheetNames.Summary} worksheet...");
        AddSummaryWorkSheet(workbook, members);
        
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

    private static IXLWorksheet AddUnallocatedWorksheet(XLWorkbook workbook, List<Error> errors)
    {
        var ws = workbook.AddWorksheet(WorksheetNames.Unallocated);

        var propertyNames = typeof(Error).GetProperties().Select(p => p.Name);
        var addedColumns = Data.Columns.Detail;

        var headers = propertyNames
            .Concat(addedColumns.Select(c => c.Header))
            .ToArray();

        for (var i = 0; i < headers.Count(); i++)
            ws.Cell(1, i + 1).SetValue(headers[i]);

        // TODO: We should really be using PopulateData here
        var rowStart = 2;
        for (var err = 0; err < errors.ToArray().Length; err++)
        {
            var error = errors[err];
            var row = err + rowStart;
            ws.Cell(row, 1).InsertData(new[] { error });

            for (var col = 0; col < addedColumns.Length; col++)
            {
                var column = addedColumns[col];
                var cell = ws.Cell(row, propertyNames.Count() + col + 1);
                column.PopulateCell(cell, error.Reference, workbook);
            }
        }

        var addedColumnRange = ws.Range(
            rowStart, 
            propertyNames.Count(), 
            errors.Count() + rowStart, 
            propertyNames.Count() + headers.Length);

        addedColumnRange.AddConditionalFormat<XLColor>(
            (style, effect) => style.Fill.SetBackgroundColor(effect),
            [
                new (AllocationStatus.Allocated.ToString(),  XLColor.GrannySmithApple),
                new (AllocationStatus.Dismiss.ToString(), XLColor.GrannySmithApple),
                new (AllocationStatus.Resolved.ToString(), XLColor.GrannySmithApple),
                new (AllocationStatus.Resolve.ToString(), XLColor.Orange),
                new (AllocationStatus.OverAllocated.ToString(), XLColor.Red)
            ]);
        
        ApplySharedFormatting(ws);
        ws.RangeUsed().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        var statusColumn = ws.GetColumnByHeader(ColumnNames.Status);
        var outcomeColumn = ws.GetColumnByHeader(ColumnNames.Outcome);
        var notesColumn = ws.GetColumnByHeader(ColumnNames.Notes);

        statusColumn.Width = 15;
        outcomeColumn.Width = 15;
        
        ws.Protect();
        Unprotect(outcomeColumn);
        Unprotect(notesColumn);

        return ws;
    }

    private static IXLWorksheet AddSummaryWorkSheet(XLWorkbook workbook, List<Member> members)
    {
        var ws = workbook.AddWorksheet(WorksheetNames.Summary);

        var detailWorksheet = workbook.Worksheet(WorksheetNames.Detail);
        var detailRange = detailWorksheet.RangeUsed();

        for (int row = 1; row <= detailRange.RowCount(); row++)
        {
            for (int column = 1; column <= detailRange.ColumnCount(); column++)
            {
                var srcAddress = detailWorksheet.Cell(row, column).Address.ToStringRelative();
                var srcValue = $"'{detailWorksheet.Name}'!{srcAddress}";

                ws.Cell(column, row).FormulaA1 = row > 1 && column > 1
                    ? ExtractDate(srcValue)
                    : srcValue;
            }
        }

        ApplySharedFormatting(ws);
        ws.Protect();

        return ws;
    }

    private IXLWorksheet AddDetailWorksheet(XLWorkbook workbook, IEnumerable<Member> members)
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
                var formattedReference = FormatReference(sub.Reference, sub.Credit, sub.Date);
                cell.SetValue(formattedReference);

                FormatAssignmentConfidence(cell, sub);

                // the csv column drops leading zeros
                if (int.Parse(sub.AccountNumber) != int.Parse(_config.DefaultAccount))
                    cell.Style.Font.SetFontColor(XLColor.Blue);
            }
        };

        PopulateData(ws, members,
            advanceDate: cell => cell.Row++,
            carriageReturn: cell => { cell.Column++; cell.Row = 1; },
            formatCell: formatCell);

        return ws;
    }

    private static IXLWorksheet PopulateData(
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
        if (sub.AssignmentConfidence == AssignmentConfidence.Medium)
            cell.Style.Font.SetFontColor(XLColor.DarkOrange);

        if (sub.AssignmentConfidence == AssignmentConfidence.Low)
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

        ws.Columns().AdjustToContents();
    }

    private static void Unprotect(IXLColumn column)
        => column.Style.Protection.SetLocked(false);

    private static string ExtractDate(string text)
    {
        // arguably better to use regex, but excel and odf do not use the same syntax
        var shortDateLength = 5; // dd/MM
        return $"=LEFT({text}, {shortDateLength})";
    }
}
