using ClosedXML.Excel;
using SubsCheck.Constants;
using SubsCheck.Constants.Enums;
using SubsCheck.Extensions.Excel;
using SubsCheck.Models;
using SubsCheck.Models.IO.Input;
using static SubsCheck.Constants.Excel;
using static SubsCheck.Helpers.Helpers;

namespace SubsCheck.Services.ExcelWriters
{
    public class WorksheetDetailBuilder(Configuration config) : WorksheetBuilder(config)
    {
        protected override void PopulateData<T>(XLWorkbook workbook, List<T> data)
        {
            var members = data as List<Member>;

            _ws.Cell(DataRowStart, MainHeaderColumnNumber).InsertData(
                members.First().Slots.Select(s => s.Date.ToString("MMM yy")));

            var memberNames = members.Select(m => $"{m.LastName} {m.FirstName}");

            _ws.Cell(MainHeaderRowNumber, DataColumnStart).InsertData(
                new IEnumerable<string>[] { memberNames});

            for (var mem = 0; mem < members.Count(); mem++)
            {
                var member = members[mem];
                for (var s = 0; s < member.Slots.Count; s++ )
                {
                    var slot = member.Slots[s];
                    var cell = _ws.Cell(s + DataRowStart, mem + DataColumnStart);
                    
                    PopulateCell(cell, slot);
                }
            }

            var lastRowUsed = _ws.LastRowUsed().RowNumber();
            _ws.Cell(lastRowUsed + 1, 1).SetValue(RowNames.Notes);
        }

        private void PopulateCell(IXLCell cell, Slot slot)
        {
            var sub = slot.Sub;

            if (!slot.IsAvailable)
            {
                cell.SetValue(CellValues.Unavailable);
            }
            else if (sub is null)
            {
                cell.SetValue(CellValues.Unpaid);
            }
            else
            {
                var formattedReference = FormatReference(sub.Reference, sub.Credit, sub.Date);
                cell.SetValue(formattedReference);

                FormatAssignmentConfidence(cell, sub);

                // the csv column drops leading zeros
                if (int.Parse(sub.AccountNumber) != int.Parse(_config.DefaultAccount))
                    cell.Style.Font.SetFontColor(XLColor.Blue);
            }
        }

        protected override List<List<ColumnProtectionStatus>> GroupProtectionStatusColumns(
            List<ColumnProtectionStatus> protectionStatusColumns)
        {
            var columnGroups = new List<List<ColumnProtectionStatus>>();

            foreach (var column in protectionStatusColumns)
                columnGroups.Add([column]);

            return columnGroups;
        }

        private static void FormatAssignmentConfidence(IXLCell cell, Subscription sub)
        {
            if (sub.AssignmentConfidence == AssignmentConfidence.Medium)
                cell.Style.Font.SetFontColor(XLColor.DarkOrange);

            if (sub.AssignmentConfidence == AssignmentConfidence.Low)
                cell.Style.Font.SetFontColor(XLColor.Red);
        }

        protected override void StyleData<T>(List<T> data)
        {
            _ws.Column(1).Width = 10;
            DataRangeUsed().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            _ws.GetRowByValue(RowNames.Notes)?.Style.ApplyStyle(Styles.Note);
        }
            
        protected override void UnprotectRange()
            => DataRangeUsed().Style.Protection.SetLocked(false);
    }
}
