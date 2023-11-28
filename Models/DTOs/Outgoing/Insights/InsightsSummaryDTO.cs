using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Insights
{
    public class InsightsSummaryDTO
    {
        public decimal MaxValue { get; set; }

        public decimal MinValue { get; set; }

        public decimal Mean { get; set; }

        public decimal Q1 { get; set; }

        public decimal Median { get; set; }

        public decimal Q3 { get; set; }
    }
}
