using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Budget
{
    public class BudgetCreatedDTO
    {
        public Guid Id { get; set; }

        public BankNameEnum? Bank { get; set; }

        public BudgetTypeEnum BudgetType { get; set; }

        public string Currency { get; set; }

        [Column(TypeName = "money")]
        public decimal? BudgetBalance { get; set; }
    }
}
