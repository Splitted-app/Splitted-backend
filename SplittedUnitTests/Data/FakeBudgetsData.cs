using Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Data
{
    public static class FakeBudgetsData
    {
        public static IQueryable<Budget> Budgets = new List<Budget>
        {
            new Budget
            {
                Id = new Guid("5b2294c4-5ab5-4ad2-b943-e4a30feb609f"),
                Bank = Models.Enums.BankNameEnum.Pekao,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 1000,
                CreationDate = DateTime.Parse("2023-12-12")
            },

            new Budget
            {
                Id = new Guid("a618304c-ad79-452a-ba6a-f54828f8a249"),
                Bank = Models.Enums.BankNameEnum.Ing,
                BudgetType = Models.Enums.BudgetTypeEnum.Family,
                Name = "FamilyBudget",
                Currency = "PLN",
                BudgetBalance = -100,
                CreationDate = DateTime.Parse("2023-11-12")
            },

            new Budget
            {
                Id = new Guid("5451431a-f252-43a4-bbf2-71508d95d563"),
                BudgetType = Models.Enums.BudgetTypeEnum.Partner,
                Name = "PartnerBudget",
                Currency = "PLN",
                BudgetBalance = 12000,
                CreationDate = DateTime.Parse("2023-10-12")
            },

            new Budget
            {
                Id = new Guid("8fec0e0f-c5e7-4dcd-9f1d-fcf769b9aa94"),
                BudgetType = Models.Enums.BudgetTypeEnum.Family,
                Name = "TemporaryBudget",
                Currency = "PLN",
                BudgetBalance = 1234.50M,
                CreationDate = DateTime.Parse("2023-09-12")
            },

            new Budget
            {
                Id = new Guid("4d688676-44e1-4e76-bfbe-4e269df1eec4"),
                Bank = Models.Enums.BankNameEnum.Santander,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 13456.34M,
                CreationDate = DateTime.Parse("2023-08-12")
            }
        }
        .AsQueryable();
    }
}
