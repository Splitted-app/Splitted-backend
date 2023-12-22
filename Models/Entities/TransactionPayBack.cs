using Models.Enums;
using Models.Interfaces;
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
    public class TransactionPayBack : IEntity
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

        public Guid? OwingUserId { get; set; }

        public User? OwingUser { get; set; } = null;

        public Guid? OwedUserId { get; set; }

        public User? OwedUser { get; set; } = null;
    }
}
