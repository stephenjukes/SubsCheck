using ClosedXML.Excel;
using SubsCheck.Models;
using SubsCheck.Services.Interfaces;
using System.Collections;

namespace SubsCheck.Services;
public class ExcelDataIO : IDataIO
{
    public Task<IEnumerable<T>> Read<T>(ReadRequest request)
    {
        throw new NotImplementedException();
    }

    public void Write<T>(WriteRequest<T> request)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Summary");
        ws.Cell(1, 1).InsertData((IEnumerable)request.Data);

        // general styling
        var range = ws.RangeUsed();
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Font.FontColor = XLColor.Charcoal;

        var column1 = ws.Column(1);
        column1.AdjustToContents();
        column1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        column1.Style.Font.SetBold();

        var row1 = ws.Row(1);
        row1.Style.Font.SetBold();
        
        // conditional formatting
        range.AddConditionalFormat()
            .WhenEquals("x")
            .Fill.SetBackgroundColor(XLColor.LightPink)
            .Font.SetFontColor(XLColor.Gray);

        range.AddConditionalFormat()
            .WhenBetween("/", "A") // ie: a number
            .Fill.SetBackgroundColor(XLColor.GrannySmithApple);

        // freeze panes
        ws.SheetView.Freeze(1, 1);


        workbook.SaveAs(request.ResourceLocator);
    }
}
