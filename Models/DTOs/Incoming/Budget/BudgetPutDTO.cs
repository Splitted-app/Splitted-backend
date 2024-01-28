using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Incoming.Budget
{
    public class BudgetPutDTO
    {
        private BankNameEnum? bank;
        private string? name;
        private decimal? budgetBalance;
        private readonly HashSet<string> setProperties = new HashSet<string>();


        public HashSet<string> SetProperties
        {
            get => new HashSet<string>(setProperties);
        }


        public BankNameEnum? Bank
        {
            get => bank;
            set
            {
                bank = value;
                setProperties.Add(nameof(Bank));
            }
        }

        public string? Name
        {
            get => name;
            set
            {
                name = value;
                setProperties.Add(nameof(Name));
            }
        }

        public decimal? BudgetBalance
        {
            get => budgetBalance;
            set
            {
                budgetBalance = value;
                setProperties.Add(nameof(BudgetBalance));
            }
        }
    }
}
