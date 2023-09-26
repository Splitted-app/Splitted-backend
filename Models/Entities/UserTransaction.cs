using Splitted_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entities
{
    [Table("UserTransactions")]
    public class UserTransaction
    {
        public Guid UserId { get; set; }

        public Guid TransactionId { get; set; }


        public User User { get; set; } = new();

        public Transaction Transaction { get; set; } = new();
    }
}
