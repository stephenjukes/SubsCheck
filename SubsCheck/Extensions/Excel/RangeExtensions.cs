using ClosedXML.Excel;
using SubsCheck.Models.Excel;

namespace SubsCheck.Extensions.Excel
{
    public static class RangeExtensions
    {
        public static void AddConditionalFormat<T>(this IXLRangeBase range, 
            Action<IXLStyle, T> render, IEnumerable<ConditionalFormatParameters<T>> parameterSets)
        {
            foreach (var parameterSet in parameterSets)
            {
                var style = range.AddConditionalFormat().WhenEquals(parameterSet.Cause);
                render(style, parameterSet.Effect);
            }
        }

        public static IXLRange AsRange(this IEnumerable<IXLCell> cells, IXLWorksheet ws)
        {
            var orderedCells = cells.OrderBy(c => 
                c.WorksheetRow().RowNumber() + c.WorksheetColumn().ColumnNumber());

            return ws.Range(orderedCells.First(), orderedCells.Last());
        }
    }
}
