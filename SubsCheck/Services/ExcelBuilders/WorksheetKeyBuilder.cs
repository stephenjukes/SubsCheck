using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using SubsCheck.Models.IO.Input;
using SubsCheck.Services.ExcelWriters;

namespace SubsCheck.Services.ExcelBuilders
{
    internal class WorksheetKeyBuilder : WorksheetBuilder
    {
        public WorksheetKeyBuilder(Configuration config) : base(config)
        {
        }

        protected override void PopulateData<T>(XLWorkbook workbook, List<T> data)
        {
            
        }

        protected override void StyleData<T>(List<T> data)
        {
            
        }

        protected override void UnprotectRange()
        {
            
        }
    }
}
