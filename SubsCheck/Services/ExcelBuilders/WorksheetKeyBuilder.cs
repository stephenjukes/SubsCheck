using ClosedXML.Excel;
using SubsCheck.Constants;
using SubsCheck.Models;
using SubsCheck.Models.IO.Input;
using SubsCheck.Services.ExcelWriters;

namespace SubsCheck.Services.ExcelBuilders
{
    public class WorksheetKeyBuilder : WorksheetBuilder
    {
        public WorksheetKeyBuilder(Configuration config) : base(config)
        {
        }

        protected override void PopulateData<T>(XLWorkbook workbook, List<T> data)
        {
            var keyItems = GetKeyItems().ToList();

            for (int i = 0; i < keyItems.Count; i++)
            {
                var keyItem = keyItems[i];
                var cell = _ws.Cell(i + MainHeaderRowNumber, 1);

                var row = new string[] { keyItem.Value, keyItem.Description };
                var dataRow = new string[][] { row };

                cell.InsertData(dataRow);
                keyItem.Style(cell.Style);
            }
        }

        private static IEnumerable<KeyItem> GetKeyItems()
        {
            var date = DateTime.Now.ToString("yyyy/MM/dd");
            var sampleReference = $"{date} (£10) joe bloggs";

            return [
                new ("Sample",
                     "Description",
                     Styles.None),

                new("ReadOnly",
                    "Read only column, (content cannot be amended",
                    Styles.ReadOnly),

                new("Editable",
                    "Editable column, (content can be amended)",
                    Styles.Editable),

                new("x",
                    "Unpaid",
                    Styles.Unpaid),

                new("-",
                    "Not a member",
                    Styles.NonMember),

                new(sampleReference,
                    "paid to a different account",
                    Styles.DifferentAccount),

                new(sampleReference,
                    "deviating from the most common reference (medium risk of being incorrectly allocated)",
                    Styles.IncorrectAllocationMediumRisk),

                new(sampleReference,
                    "single reference of its kind (high risk of being incorrectly allocated)",
                    Styles.IncorrectAllocationHighRisk)
            ];
        }
    }
}
