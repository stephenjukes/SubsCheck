using ClosedXML.Excel;
using SubsCheck.Constants.Enums;
using SubsCheck.Models;
using SubsCheck.Models.Excel;
using SubsCheck.Models.IO.Input;

namespace SubsCheck.Services.ExcelWriters
{
    public abstract class WorksheetBuilder
    {
        protected IXLWorksheet _ws;

        protected readonly Configuration _config;
        protected const string Unavailable = "-";
        protected const string Unpaid = "x";
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
            AddProtectionStatusBanner();
            
            return _ws;
        }

        protected abstract void PopulateData<T>(XLWorkbook workbook, List<T> data);

        protected abstract void StyleData<T>(List<T> data);

        private ProtectionStatus GetReadWriteStatus(IEnumerable<IXLCell> cells)
        {
            if (cells.All(c => c.Style.Protection.Locked))
                return ProtectionStatus.ReadOnly;

            if (cells.All(c => !c.Style.Protection.Locked))
                return ProtectionStatus.Editable;

            return ProtectionStatus.SomeEditable;
        }

        protected IXLWorksheet PopulateData(
            IEnumerable<Member> members,
            Action<Cell> advanceDate,
            Action<Cell> carriageReturn,
            Action<IXLCell, Slot> formatCell)
        {
            // a bit sloppy
            var dateHeaders = GetDateHeaders(members.First());
            var maxScope = Math.Max(dateHeaders.Count() + 1, members.Count() + 1);
            var range = _ws.Range(1, 1, maxScope, maxScope);

            var cellPosition = new Cell 
            { 
                Row = MainHeaderRowNumber, 
                Column = MainHeaderColumnNumber 
            };

            foreach (var dateHeader in dateHeaders)
            {
                _ws.Cell(cellPosition.Row, cellPosition.Column).Value = dateHeader;
                advanceDate(cellPosition);
            }

            carriageReturn(cellPosition);

            foreach (var member in members)
            {
                var rowHeaders = GetNameHeader(member);

                foreach (var rowHeader in rowHeaders)
                {
                    _ws.Cell(cellPosition.Row, cellPosition.Column).Value = rowHeader;
                    advanceDate(cellPosition);
                }

                foreach (var slot in member.Slots)
                {
                    var cell = _ws.Cell(cellPosition.Row, cellPosition.Column);
                    formatCell(cell, slot);

                    advanceDate(cellPosition);
                }

                carriageReturn(cellPosition);
            }

            return _ws;
        }

        public void AddProtectionStatusBanner()
        {
            var protectionStatusByColumn = DataRangeUsed().Cells()
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

            //// TODO: Try to separate styling from here
            _ws.Row(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
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

        protected static IEnumerable<string> GetDateHeaders(Member member)
            => GetNameHeader(member).Select(v => "")
        .Concat(member.Slots.Select(s => s.Date.ToString("MMM yy")));

        protected static IEnumerable<string> GetNameHeader(Member member)
            => [$"{member.LastName} {member.FirstName}"];

        protected IXLColumn? GetColumnByHeader(string header)
        {
            var rangeUsed = _ws.RangeUsed();
            return rangeUsed
                ?.Row(MainHeaderColumnNumber)
                ?.CellsUsed().FirstOrDefault(c => c.GetString() == header)
                ?.WorksheetColumn();
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

        protected void ApplySharedFormatting(IXLRange range)
        {
            if (range is null) return;

            _ws.Rows(1, HeaderRowCount).Style.Font.SetBold();
            _ws.Columns(1, HeaderColumnCount).Style.Font.SetBold();
            _ws.Columns(1, HeaderColumnCount).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            _ws.SheetView.Freeze(HeaderRowCount, HeaderColumnCount);

            range.AddConditionalFormat()
                .WhenEquals(Unpaid)
                .Fill.SetBackgroundColor(XLColor.LightPink)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            range.AddConditionalFormat()
                .WhenEquals(Unavailable)
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Font.SetFontColor(XLColor.LightGray);

            _ws.Row(1).AddConditionalFormat()
                .WhenEquals(ProtectionStatus.ReadOnly.ToString())
                .Fill.SetBackgroundColor(XLColor.Orange);

            _ws.Row(1).AddConditionalFormat()
                .WhenEquals(ProtectionStatus.Editable.ToString())
                .Fill.SetBackgroundColor(XLColor.GrannySmithApple);

            _ws.Columns().AdjustToContents();
        }
    }
}
