using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubsCheck.Helpers.DateHelper.dto;

namespace SubsCheck.Helpers.DateHelper
{
    public static class DateHelper
    {
        public static IEnumerable<Month> Months => Enumerable.Range(1, 12)
            .Select(i => new Month
            {
                Number = i,
                Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)
            });
    }
}
