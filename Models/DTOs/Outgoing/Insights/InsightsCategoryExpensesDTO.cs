using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Insights
{
    public class InsightsCategoryExpensesDTO
    {
        public string? CategoryName { get; set; }

        [Column(TypeName = "money")]
        public decimal Expenses { get; set; }
    }
}
