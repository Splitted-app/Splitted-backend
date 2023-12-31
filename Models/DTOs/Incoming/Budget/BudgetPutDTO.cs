﻿using Models.Enums;
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
        private string? currency;
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

        public string? Currency
        {
            get => currency;
            set
            {
                currency = value;
                setProperties.Add(nameof(Currency));
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
