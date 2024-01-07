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
    [Table("Budgets")]
    public class Budget : IEntity, ICloneable
    {
        public Guid Id { get; set; }

        public BankNameEnum? Bank { get; set; }

        public BudgetTypeEnum BudgetType { get; set; }

        public string Name { get; set; }

        public string Currency { get; set; }

        [Column(TypeName = "money")]
        public decimal BudgetBalance { get; set; }

        public DateTime CreationDate { get; set; }



        public List<User> Users { get; set; } = new();

        public List<UserBudget> UserBudgets { get; set; } = new();

        public List<Transaction> Transactions { get; set; } = new();


        public object Clone()
        {
            return new Budget
            {
                Id = Id,
                Bank = Bank,
                BudgetType = BudgetType,
                Name = Name,
                Currency = Currency,
                BudgetBalance = BudgetBalance,
                CreationDate = CreationDate,
                Users = Users,
                UserBudgets = UserBudgets,
                Transactions = Transactions.ConvertAll(t => (Transaction)t.Clone()),
            };
        }
    }
}
