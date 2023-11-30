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
            for (DateTime month = dateFrom.Date; month.Date <= dateTo.Date; month = month.AddMonths(1))
            {
                yield return month;
            }
        }
    }
}
