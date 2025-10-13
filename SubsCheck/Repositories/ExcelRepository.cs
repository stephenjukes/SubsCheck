using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using SubsCheck.Services.dto;

namespace SubsCheck.Repositories
{
    public class ExcelRepository : IRepository
    {
        public Task<IEnumerable<T>> GetAll<T>(GetRequest request)
        {
            //using (var workbook = new XLWorkbook(request.Path))
            //{
            //    var ws = workbook.Worksheet(1);

            //    var headers = ws
            //        .FirstRowUsed()
            //        ?.CellsUsed()
            //        .Select(c => c.GetString().Trim())
            //        .ToArray();

            //    if (headers is null)
            //        throw new InvalidOperationException($"input is {Path.GetFileName(request.Path)} is empty");

            //    var data = ws
            //        .RowsUsed()
            //        .Skip(1);

            //    foreach (var row in data)
            //    {
            //        var entity = new T();

            //        foreach (var cell in row.CellsUsed())
            //        {
            //            var header = headers[cell.Address.ColumnNumber - 1];
            //        }
            //    }
            //}

            return null;
        }
    }
}
