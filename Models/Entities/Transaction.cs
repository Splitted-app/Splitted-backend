using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Enums;
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
    public class Transaction
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


        public Guid BudgetId { get; set; }

        public Budget Budget { get; set; } = null!;

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public Guid? DuplicatedTransactionId { get; set; }

        public Transaction? DuplicatedTransaction { get; set; } = null;


        public override bool Equals(object? obj)
        {
            if (obj is Transaction transaction)
                return transaction.Amount.Equals(Amount) && transaction.Currency.Equals(Currency) &&
                    transaction.Date.Equals(Date) && transaction.Description.Equals(Description);

            return false;
        }

        public override int GetHashCode()
        {
            return (Amount, Currency, Date, Description).GetHashCode();
        }

    }
}
