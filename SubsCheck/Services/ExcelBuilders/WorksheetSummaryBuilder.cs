using ClosedXML.Excel;
using SubsCheck.Models.IO.Input;
using static SubsCheck.Constants.Excel;

namespace SubsCheck.Services.ExcelWriters
{
    public class WorksheetSummaryBuilder(Configuration config) : WorksheetBuilder(config)
    {
        protected override void PopulateData<T>(XLWorkbook workbook, List<T> data)
        {
            var detailWorksheet = workbook.Worksheet(WorksheetNames.Detail);
            var detailRange = detailWorksheet.RangeUsed();

            // not sure why HeaderColumnCount is added to both, but it seems to work
            for (int row = MainHeaderRowNumber; row < detailRange.RowCount() + HeaderColumnCount; row++)
            {
                for (int column = MainHeaderColumnNumber; column < detailRange.ColumnCount() + HeaderColumnCount; column++)
                {
                    var srcAddress = detailWorksheet.Cell(row, column).Address.ToStringRelative();
                    var srcValue = $"'{detailWorksheet.Name}'!{srcAddress}";

                    var pivotedRow = column + MainHeaderColumnNumber;
                    var pivotedColumn = row - MainHeaderColumnNumber;

                    var minimumColumnNumber = 1;
                    if (pivotedColumn < minimumColumnNumber) continue;

                    var isDataCell = pivotedRow > MainHeaderRowNumber && pivotedColumn > MainHeaderColumnNumber;
                    _ws.Cell(pivotedRow, pivotedColumn).FormulaA1 = isDataCell
                        ? ExtractDate(srcValue)
                        : srcValue;
                }
            }
        }

        protected override void StyleData<T>(List<T> data)
            => DataRangeUsed().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        private static string ExtractDate(string text)
        {
            // arguably better to use regex, but xls and ods do not use the same syntax
            var shortDateLength = 5; // dd/MM
            return $"=LEFT({text}, {shortDateLength})";
        }

        protected override void UnprotectRange()
        {
            // leave as readonly
        }
    }
}
