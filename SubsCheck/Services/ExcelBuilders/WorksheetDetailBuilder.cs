using System;
using ClosedXML.Excel;
using SubsCheck.Models;
using SubsCheck.Models.Constants.Enums;
using SubsCheck.Models.IO.Input;
using static SubsCheck.Helpers.Helpers;

namespace SubsCheck.Services.ExcelWriters
{
    public class WorksheetDetailBuilder(Configuration config) : WorksheetBuilder(config)
    {
        protected override void PopulateData<T>(XLWorkbook workbook, List<T> data)
        {
            var members = data as IEnumerable<Member>;

            Action<IXLCell, Slot> formatCell = (cell, slot) =>
            {
                var sub = slot.Sub;

                if (!slot.IsAvailable)
                {
                    cell.SetValue(Unavailable);
                }
                else if (sub is null)
                {
                    cell.SetValue(Unpaid);
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
            };

            PopulateData(members,
                advanceDate: cell => cell.Row++,
                carriageReturn: cell => { cell.Column++; cell.Row = MainHeaderRowNumber; },
                formatCell: formatCell);
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
            => DataRangeUsed().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        protected override void UnprotectRange()
            => DataRangeUsed().Style.Protection.SetLocked(false);
    }
}
