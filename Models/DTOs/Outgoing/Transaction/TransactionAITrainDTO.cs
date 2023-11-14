﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Transaction
{
    public class TransactionAITrainDTO
    {
        public string Date { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public string? BankCategory { get; set; }

        public string? UserCategory { get; set; }
    }
}
