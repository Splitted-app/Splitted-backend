using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.DTOs.Outgoing.Transaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Budget
{
    public class BudgetTransactionsGetDTO
    {
        public List<TransactionGetDTO> Transactions { get; set; }

        [Column(TypeName = "money")]
        public decimal Income { get; set; } = 0;

        [Column(TypeName = "money")]
        public decimal Expenses { get; set; } = 0;

        [Column(TypeName = "money")]
        public decimal Debt { get; set; } = 0;
    }
}
