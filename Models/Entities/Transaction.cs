﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Enums;
using Models.Interfaces;
using Splitted_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entities
{
    [Table("Transactions")]
    public class Transaction : IEntity, ICloneable
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

        public bool ToCancel { get; set; }


        public Guid BudgetId { get; set; }

        public Budget Budget { get; set; } = null!;

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public Guid? DuplicatedTransactionId { get; set; }

        public Transaction? DuplicatedTransaction { get; set; } = null;

        public List<Transaction> DuplicatedTransactions { get; set; } = new();

        public List<TransactionPayBack> TransactionPayBacks { get; set; } = new();



        public override bool Equals(object? obj)
        {
            if (obj is Transaction transaction)
                return transaction.Amount.Equals(Amount) && transaction.Currency.Equals(Currency) &&
                    transaction.Date.Equals(Date) && transaction.Description.Equals(Description);

            return false;
        }

        public Transaction Copy()
            => new Transaction
            {
                Amount = Amount,
                Currency = Currency,
                Date = Date,
                Description = Description,
                TransactionType = TransactionType,
                BankCategory = BankCategory,
                AutoCategory = AutoCategory,
                UserCategory = UserCategory,
                UserId = UserId,
            };

        public override int GetHashCode()
        {
            return (Amount, Currency, Date, Description).GetHashCode();
        }

        public object Clone()
        {
            return new Transaction
            {
                Id = Id,
                Amount = Amount,
                Currency = Currency,
                Date = Date,
                Description = Description,
                TransactionType = TransactionType,
                BankCategory = BankCategory,
                AutoCategory = AutoCategory,
                UserCategory = UserCategory,
                BudgetId = BudgetId,
                UserId = UserId,
                DuplicatedTransactionId = DuplicatedTransactionId,
                DuplicatedTransactions = DuplicatedTransactions,
            };
        }
    }
}
