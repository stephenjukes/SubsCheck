using ClosedXML.Excel;

namespace SubsCheck.Extensions.Excel
{
    public static class WorksheetExtensions
    {
        public static IXLColumn GetColumnByHeader(this IXLWorksheet ws, string header)
        {
            var headers = ws.Row(1);

            var cell = headers.CellsUsed().FirstOrDefault(c => c.GetString() == header);

            var columnNumber = cell.Address.ColumnNumber;

            return ws.Column(columnNumber);
        }
    }
}
