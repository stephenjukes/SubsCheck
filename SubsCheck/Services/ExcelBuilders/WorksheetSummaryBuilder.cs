using System.Globalization;
using ClosedXML.Excel;
using SubsCheck.Constants;
using SubsCheck.Extensions.Excel;
using SubsCheck.Models.IO.Input;
using static SubsCheck.Constants.Excel;

namespace SubsCheck.Services.ExcelWriters
{
    public class WorksheetSummaryBuilder(Configuration config) : WorksheetBuilder(config)
    {
        protected override void PopulateData<T>(XLWorkbook workbook, List<T> data)
        {
            var detailWorksheet = workbook.Worksheet(WorksheetNames.Detail);
            var detailRange = detailWorksheet.RangeUsed()?.Cells();
            var notesRow = detailWorksheet.GetRowByValue(RowNames.Notes);
            var forPopulation = detailRange?.Where(c => c.WorksheetRow() != notesRow).AsRange(detailWorksheet);

            // not sure why HeaderColumnCount is added to both, but it seems to work
            // the '1' is an offset, not the HeaderColumnCount
            for (int row = MainHeaderRowNumber; row < forPopulation.RowCount() + HeaderColumnCount; row++)
            {
                for (int column = MainHeaderColumnNumber; column < forPopulation.ColumnCount() + HeaderColumnCount; column++)
                {
                    var srcAddress = detailWorksheet.Cell(row, column).Address.ToStringRelative();
                    var srcValue = $"'{detailWorksheet.Name}'!{srcAddress}";

                    var pivotedRow = column + MainHeaderColumnNumber;
                    var pivotedColumn = row - MainHeaderColumnNumber;

                    var minimumColumnNumber = 1;
                    if (pivotedColumn < minimumColumnNumber) continue;

                    var cellValue = detailWorksheet.Cell(row, column).GetValue<string>();

                    // get culture info from config
                    var isDataCell = cellValue.Length >= 10 && DateTime.TryParse(cellValue[..10], new CultureInfo("en-GB"), out var date); // pivotedRow > MainHeaderRowNumber && pivotedColumn > MainHeaderColumnNumber;
                    _ws.Cell(pivotedRow, pivotedColumn).FormulaA1 = isDataCell
                        ? ExtractDate(srcValue)
                        : srcValue;
                }

                var lastColumnUsed = _ws.LastColumnUsed();
                _ws.Cell(MainHeaderRowNumber, lastColumnUsed.ColumnNumber() + 1).SetValue(ColumnNames.Notes);
            }
        }

        protected override void StyleData<T>(List<T> data)
        {
            var dataRangeUsed = DataRangeUsed();

            dataRangeUsed.Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var notesColumn = _ws.GetColumnByValue(ColumnNames.Notes);
            notesColumn.Width = 50; // TODO: make this a constant

            var notesData = _ws.Range(
                DataRowStart,
                notesColumn.ColumnNumber(),
                dataRangeUsed.RowCount(),
                notesColumn.ColumnNumber());
           
            notesData.Style.ApplyStyle(Styles.Note);
        }

        private static string ExtractDate(string text)
        {
            // arguably better to use regex, but xls and ods do not use the same syntax
            var shortDateLength = 5; // dd/MM
            return $"=LEFT({text}, {shortDateLength})";
        }

        protected override void UnprotectRange()
            => _ws.GetColumnByValue(ColumnNames.Notes)?.Style.Protection.SetLocked(false);
    }
}
