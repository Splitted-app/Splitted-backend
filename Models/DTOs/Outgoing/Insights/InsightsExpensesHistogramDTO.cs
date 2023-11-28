using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Insights
{
    public class InsightsExpensesHistogramDTO
    {
        public string Range { get; set; }

        public int Count { get; set; }
    }
}
