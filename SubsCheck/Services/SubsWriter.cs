using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using SubsCheck.Models;
using SubsCheck.Models.IO.Input;
using SubsCheck.Services.ExcelBuilders;
using SubsCheck.Services.ExcelWriters;
using SubsCheck.Services.Interfaces;
using static SubsCheck.Constants.Excel;

namespace SubsCheck.Services;
public class SubsWriter : ISubsWriter
{
    private readonly WorksheetKeyBuilder _worksheetKeyBuilder;
    private readonly WorksheetDetailBuilder _worksheetDetailBuilder;
    private readonly WorksheetUnallocatedBuilder _worksheetUnallocatedBuilder;
    private readonly WorksheetSummaryBuilder _worksheetSummaryBuilder;

    public SubsWriter(Configuration config)
    {
        _worksheetKeyBuilder = new WorksheetKeyBuilder(config);
        _worksheetDetailBuilder = new WorksheetDetailBuilder(config);
        _worksheetUnallocatedBuilder = new WorksheetUnallocatedBuilder(config);
        _worksheetSummaryBuilder = new WorksheetSummaryBuilder(config);
    }

    public void Write(WriteRequest<Member, UnallocatedSub> request)
    {
        var data = request.Data.ToList();
        using var workbook = new XLWorkbook();

        var worksheetKey = _worksheetKeyBuilder.Create(WorksheetNames.Key, workbook, new List<string>());
        var worksheetDetail = _worksheetDetailBuilder.Create(WorksheetNames.Detail, workbook, data);
        var worksheetUnallocated = _worksheetUnallocatedBuilder.Create(WorksheetNames.Unallocated, workbook, request.Errors);
        var worksheetSummary = _worksheetSummaryBuilder.Create(WorksheetNames.Summary, workbook, data);

        var orderedWorksheets = new List<IXLWorksheet>
        {
            worksheetKey,
            worksheetUnallocated,
            worksheetDetail,
            worksheetSummary
        };

        foreach (var ws in orderedWorksheets)
            ws.Position = orderedWorksheets.IndexOf(ws) + 1;

        // must be created after detail due to referencing, but would be more user friendly to display before
        worksheetUnallocated.Position = 2;
        
        try
        {
            Console.WriteLine("Generating file...");

            workbook.SaveAs(request.ResourceLocator);

            Console.WriteLine($"File generated. \n\nYou can view the generated file at {Path.GetFullPath(request.ResourceLocator)}");
        }
        catch (Exception)
        {
            Console.WriteLine("" +
                "An error has occurred, likely because the output document is still open. " +
                "Please ensure the output document is closed and try again.");
        }
    }
}
