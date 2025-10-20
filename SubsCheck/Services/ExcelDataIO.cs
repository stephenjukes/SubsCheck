using ClosedXML.Excel;
using SubsCheck.Models;
using SubsCheck.Services.Interfaces;

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
        ws.Cell("A1").Value = "Hello World!";
        workbook.SaveAs(request.ResourceLocator);
    }
}
