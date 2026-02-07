using ClosedXML.Excel;
using SubsCheck.Constants;
using SubsCheck.Constants.Enums;
using SubsCheck.Extensions.Excel;
using SubsCheck.Models;
using SubsCheck.Models.IO.Input;
using static SubsCheck.Constants.Excel;

namespace SubsCheck.Services.ExcelWriters
{
    public abstract class WorksheetBuilder
    {
        protected IXLWorksheet _ws;

        protected readonly Configuration _config;
        protected const int HeaderRowCount = 2;
        protected const int HeaderColumnCount = 1;
        protected const int ReadWriteHeaderRowNumber = 1;
        protected const int MainHeaderRowNumber = 2;
        protected const int MainHeaderColumnNumber = 1;
        protected const int DataRowStart = HeaderRowCount + 1;
        protected const int DataColumnStart = HeaderColumnCount + 1;

        public WorksheetBuilder(Configuration config)
        {
            _config = config;
        }

        public IXLWorksheet Create<T>(string name, XLWorkbook workbook, List<T> data)
        {
            Console.WriteLine($"Creating {name} worksheet...");
            _ws = workbook.AddWorksheet(name);

            _ws.Protect();
            PopulateData(workbook, data);
            ApplySharedFormatting();
            StyleData(data);
            UnprotectRange();
            AddProtectionStatusBanner();
            
            return _ws;
        }

        protected abstract void PopulateData<T>(XLWorkbook workbook, List<T> data);

        protected virtual void UnprotectRange()
        {
        }

        protected virtual void StyleData<T>(List<T> data)
        {
        }

        protected virtual void AddProtectionStatusBanner()
        {
            var rangeUsed = _ws.RangeUsed();

            if (rangeUsed is null)
                return;

            var protectionStatusByColumn = _ws.Range(DataRowStart, 1, rangeUsed.RowCount(), rangeUsed.ColumnCount()).Cells()
                .GroupBy(cell => cell.WorksheetColumn().ColumnNumber())
                .Select(group =>
                {
                    var protectionStatus = GetReadWriteStatus(group);
                    return new ColumnProtectionStatus(group.Key, protectionStatus);
                })
                .OrderBy(column => column.ColumnNumber)
                .ToList();

            var columnGroups = GroupProtectionStatusColumns(protectionStatusByColumn);

            foreach (var group in columnGroups)
            {
                _ws.Range(1, group.First().ColumnNumber, 1, group.Last().ColumnNumber)
                    .Merge()
                    .Value = group.First().ProtectionStatus.ToString();
            }

            // TODO: Try to separate styling from here
            _ws.Row(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        private ProtectionStatus GetReadWriteStatus(IEnumerable<IXLCell> cells)
        {
            if (cells.All(c => c.Style.Protection.Locked))
                return ProtectionStatus.ReadOnly;

            if (cells.All(c => !c.Style.Protection.Locked))
                return ProtectionStatus.Editable;

            return ProtectionStatus.SomeEditable;
        }

        protected virtual List<List<ColumnProtectionStatus>> GroupProtectionStatusColumns(
            List<ColumnProtectionStatus> protectionStatusColumns)
        {
            ProtectionStatus previousProtectionStatus = ProtectionStatus.None;
            var columnGroups = new List<List<ColumnProtectionStatus>>();
            var columnGroup = new List<ColumnProtectionStatus>();
            foreach (var column in protectionStatusColumns)
            {
                if (column.ProtectionStatus == previousProtectionStatus)
                {
                    columnGroup.Add(column);
                }
                else
                {
                    columnGroup = [column];
                    columnGroups.Add(columnGroup);
                }

                previousProtectionStatus = column.ProtectionStatus;
            }

            return columnGroups;
        }

        protected IXLRange DataRangeUsed()
        {
            var rangeUsed = _ws.RangeUsed();

            return _ws.Range(
                DataRowStart, 
                DataColumnStart, 
                rangeUsed.LastRowUsed().RowNumber(), 
                rangeUsed.LastColumnUsed().ColumnNumber());
        }

        protected void ApplySharedFormatting()
            => ApplySharedFormatting(_ws.RangeUsed());

        protected virtual void ApplySharedFormatting(IXLRange range)
        {
            if (range is null) return;

            _ws.Rows(1, HeaderRowCount).Style.Font.SetBold();
            _ws.Columns(1, HeaderColumnCount).Style.Font.SetBold();
            _ws.Columns(1, HeaderColumnCount).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            _ws.SheetView.Freeze(HeaderRowCount, HeaderColumnCount);

            range.AddConditionalFormat()
                .WhenEquals(0)
                .Font.SetFontColor(XLColor.White);

            range.AddConditionalFormat()
                .WhenEquals(CellValues.Unpaid)
                .ApplyStyle(Styles.Unpaid);

            range.AddConditionalFormat()
                .WhenEquals(CellValues.Unavailable)
                .ApplyStyle(Styles.NonMember);

            _ws.Row(1).AddConditionalFormat()
                .WhenEquals(ProtectionStatus.ReadOnly.ToString())
                .ApplyStyle(Styles.ReadOnly);

            _ws.Row(1).AddConditionalFormat()
                .WhenEquals(ProtectionStatus.Editable.ToString())
                .ApplyStyle(Styles.Editable);

            _ws.ColumnsUsed().AdjustToContents();
        }
    }
}
