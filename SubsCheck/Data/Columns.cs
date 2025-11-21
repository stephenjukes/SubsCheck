using ClosedXML.Excel;
using SubsCheck.Constants.Enums;
using SubsCheck.Extensions.Excel;
using SubsCheck.Models.Excel;
using static SubsCheck.Constants.Excel;

namespace SubsCheck.Data
{
    public static class Columns
    {
        public static Column[] Detail =
        [
            new Column(ColumnNames.Allocated, (cell, value, workbook) =>
                {
                    var detailWorksheet = workbook.Worksheet(WorksheetNames.Detail);
                    var detailUsed = detailWorksheet.RangeUsed();
                    var detailUsedAddress = detailUsed.RangeAddress.ToString();
                    var referenceCount = $"=COUNTIF({detailWorksheet.Name}!{detailUsedAddress}, \"{value}\")";
                    cell.FormulaA1 = referenceCount;
                }),
            new Column(ColumnNames.Status, (cell, value, workbook) =>
                {
                    // this is the nicest assuming we know the positions, otherwise get by column header
                    var totalSubs = cell.CellLeft(2);
                    var allocated = cell.CellLeft(1);

                    var outcome = $"=IF({allocated}={totalSubs}" +
                        $",\"{AllocationStatus.Allocated}\" " +
                        $",IF({allocated}>{totalSubs}" +
                            $",\"{AllocationStatus.OverAllocated}\"" +
                            $",\"\"))";

                    cell.FormulaA1 = outcome;

                    var statusData = cell.WorksheetColumn().ColumnUsed();
                    statusData.Style.Font.SetBold();

                    statusData.AddConditionalFormat<XLColor>(
                        (style, effect) => style.Font.SetFontColor(effect),
                        [
                            new (AllocationStatus.Allocated.ToString(), XLColor.Green),
                            new (AllocationStatus.OverAllocated.ToString(), XLColor.Red)
                        ]);
                }),
            new Column(ColumnNames.Outcome, (cell, value, workbook) =>
                {
                    var options = new AllocationStatus[]
                    {
                        AllocationStatus.Dismiss,
                        AllocationStatus.Resolve,
                        AllocationStatus.Resolved,
                        AllocationStatus.Allocated,
                        AllocationStatus.OverAllocated
                    };

                    var dataValidation = cell.CreateDataValidation();
                    dataValidation.List(string.Join(",", options.Select(o => $"\"{o}\"")));

                    // TODO: This is repeated - DRY up
                    var totalSubs = cell.CellLeft(3);
                    var allocated = cell.CellLeft(2);

                    var outcome = $"=IF({allocated}={totalSubs}" +
                        $",\"{AllocationStatus.Allocated}\" " +
                        $",IF({allocated}>{totalSubs}" +
                            $",\"{AllocationStatus.OverAllocated}\"" +
                            $",\"\"))";

                    cell.FormulaA1 = outcome;

                    var statusData = cell.WorksheetColumn().ColumnUsed();
                    statusData.Style.Font.SetBold();
                }),

            new Column(ColumnNames.Notes, (cell, value, ws) => { })
        ];
    }
}