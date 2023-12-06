using Models.Enums;
using Splitted_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entities
{
    [Table("TransactionPayBacks")]
    public class TransactionPayBack
    {
        public Guid Id { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        public TransactionPayBackStatusEnum TransactionPayBackStatus { get; set; }

        public bool InCash { get; set; }


        public Guid OriginalTransactionId { get; set; }

        public Transaction OriginalTransaction { get; set; } = null!;

        public Guid? PayBackTransactionId { get; set; }

        public Transaction? PayBackTransaction { get; set; } = null;

        public Guid? UserId { get; set; }

        public User? User { get; set; } = null;
    }
}
