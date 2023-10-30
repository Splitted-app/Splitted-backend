using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Transaction
{
    public class TransactionGetDTO
    {
        public Guid Id { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        public string? BankCategory { get; set; }

        public string? AutoCategory { get; set; }

        public string? UserCategory { get; set; }

        public string UserName { get; set; }

        public string? AvatarImage { get; set; }
    }
}
