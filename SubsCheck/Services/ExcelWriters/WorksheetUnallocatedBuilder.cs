using ClosedXML.Excel;
using SubsCheck.Constants.Enums;
using SubsCheck.Extensions.Excel;
using SubsCheck.Models;
using SubsCheck.Models.Excel;
using SubsCheck.Models.IO.Input;
using static SubsCheck.Constants.Excel;

namespace SubsCheck.Services.ExcelWriters
{
    public class WorksheetUnallocatedBuilder : WorksheetBuilder
    {
        private readonly IEnumerable<string> _propertyNames;
        private readonly Column[] _addedColumns;
        private readonly string[] _headerNames;

        public WorksheetUnallocatedBuilder(Configuration config) : base(config)
        {
            _propertyNames = typeof(UnallocatedSub).GetProperties().Select(p => p.Name);

            _addedColumns = AddedColumns();

            _headerNames = _propertyNames
                .Concat(_addedColumns.Select(c => c.Header))
                .ToArray();
        }

        protected override void PopulateData<T>(XLWorkbook workbook, List<T> data)
        {
            var errors = data as List<UnallocatedSub>;

            for (var i = 0; i < _headerNames.Length; i++)
                _ws.Cell(2, i + 1).SetValue(_headerNames[i]);

            // TODO: We should really be using PopulateData here
            for (var err = 0; err < errors.Count; err++)
            {
                var error = errors[err];
                var row = err + DataRowStart;
                _ws.Cell(row, 1).InsertData(new[] { error });

                for (var col = 0; col < _addedColumns.Length; col++)
                {
                    var column = _addedColumns[col];
                    var cell = _ws.Cell(row, _propertyNames.Count() + col + 1);
                    column.PopulateCell(cell, error.Reference, workbook);
                }
            }
        }

       protected override void StyleData<T>(List<T> data)
       {
            var errors = data as List<UnallocatedSub>;

            var addedColumnRange = _ws.Range(
                DataRowStart,
                _propertyNames.Count(),
                DataRowStart + errors.Count(),
                _propertyNames.Count() + _headerNames.Count());

            addedColumnRange.AddConditionalFormat<XLColor>(
                (style, effect) => style.Fill.SetBackgroundColor(effect),
                [
                    new (AllocationStatus.Allocated.ToString(),  XLColor.GrannySmithApple),
                    new (AllocationStatus.Dismiss.ToString(), XLColor.GrannySmithApple),
                    new (AllocationStatus.Resolved.ToString(), XLColor.GrannySmithApple),
                    new (AllocationStatus.Resolve.ToString(), XLColor.Orange),
                    new (AllocationStatus.OverAllocated.ToString(), XLColor.Red)
                ]);

            var rangeUsed = _ws.RangeUsed();
            var lastCell = rangeUsed.LastCellUsed();

            rangeUsed.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            var accountNumberColumn = GetColumnByHeader(nameof(UnallocatedSub.AccountNumber));
            var totalSubsColumn = GetColumnByHeader(nameof(UnallocatedSub.TotalSubs));
            var allocatedColumn = GetColumnByHeader(ColumnNames.Allocated);
            var statusColumn = GetColumnByHeader(ColumnNames.Status);
            var outcomeColumn = GetColumnByHeader(ColumnNames.Outcome);
            var notesColumn = GetColumnByHeader(ColumnNames.Notes);

            DataRangeUsed().AddConditionalFormat()
                .WhenIsTrue($"=$B{DataRowStart}<>\"{_config.DefaultAccount.TrimStart('0')}\"")
                .Font.SetFontColor(XLColor.Blue);

            foreach (var column in new IXLColumn[] { totalSubsColumn, allocatedColumn, statusColumn, outcomeColumn })
                column.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            foreach (var column in new IXLColumn[] { statusColumn, outcomeColumn })
                column.Width = 15;

            notesColumn.Width = 50;

            foreach (var column in new IXLColumn[] { outcomeColumn, notesColumn })
                column.Style.Protection.SetLocked(false);
        }

        private static Column[] AddedColumns()
        {
            return [
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
}
