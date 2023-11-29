using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Insights
{
    public class InsightsBalanceHistoryDTO
    {
        public string Date { get; set; }

        public decimal Balance { get; set; }
    }
}
