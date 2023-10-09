using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Incoming.Transaction
{
    public class TransactionPostDTO
    {
        [Column(TypeName = "money"), Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public TransactionTypeEnum TransactionType { get; set; }

        public string? UserCategory { get; set; }
    }
}
