using ClosedXML.Excel;

namespace SubsCheck.Models.Excel
{
    public class Column
    {
        public Column(string header, Action<IXLCell, string, IXLWorkbook> populateCell)
        {
            Header = header;
            PopulateCell = populateCell;
        }

        public string Header { get; set; }

        public Action<IXLCell, string, IXLWorkbook> PopulateCell { get; set; }
    }
}
