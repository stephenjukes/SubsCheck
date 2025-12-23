using ClosedXML.Excel;

namespace SubsCheck.Extensions.Excel
{
    public static class WorksheetExtensions
    {
        public static IXLCell? GetCellByValue(this IXLWorksheet ws, string value)
            => ws.RangeUsed()
                ?.Cells()
                .FirstOrDefault(c => c.GetValue<string>() == value);

        public static IXLColumn? GetColumnByValue(this IXLWorksheet ws, string value)
            => ws.GetCellByValue(value)?.WorksheetColumn();

        public static IXLRow? GetRowByValue(this IXLWorksheet ws, string value)
            => ws.GetCellByValue(value)?.WorksheetRow();
    }
}
