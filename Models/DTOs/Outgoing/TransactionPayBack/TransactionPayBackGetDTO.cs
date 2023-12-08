using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.DTOs.Outgoing.Transaction;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.TransactionPayBack
{
    public class TransactionPayBackGetDTO
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public TransactionPayBackStatusEnum TransactionPayBackStatus { get; set; }

        public bool InCash { get; set; }

        public TransactionDuplicatedGetDTO? PayBackTransaction { get; set; }

        public string UserName { get; set; }

        public string? AvatarImage { get; set; }
    }
}
