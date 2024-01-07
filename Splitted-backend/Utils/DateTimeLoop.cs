using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Splitted_backend.Utils
{
    public static class DateTimeLoop
    {
        public static IEnumerable<DateTime> EachDay(DateTime dateFrom, DateTime dateTo)
        {
            for (DateTime day = dateFrom.Date; day.Date <= dateTo.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        public static IEnumerable<DateTime> EachMonth(DateTime dateFrom, DateTime dateTo)
        {
            DateTime start = new DateTime(dateFrom.Year, dateFrom.Month, 1);
            DateTime end = new DateTime(dateTo.Year, dateTo.Month, 1);

            for (; start <= end; start = start.AddMonths(1))
            {
                yield return start;
            }
        }
    }
}
