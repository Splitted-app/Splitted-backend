using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Incoming.Budget
{
    public class BudgetPostDTO
    {
        public BankNameEnum? Bank { get; set; }

        [Required]
        public string Currency { get; set; }

        [Column(TypeName = "money")]
        public decimal? BudgetBalance { get; set; }
    }
}
