using Microsoft.AspNetCore.Identity;
using Models.DTOs.Outgoing.Budget;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;

namespace Splitted_backend.Managers
{
    public static class ModeManager
    {
        public static async Task<Budget> CreateFamilyMode(IRepositoryWrapper repositoryWrapper, User firstUser,
            User secondUser, string currency, BankNameEnum? bank)
        {
            Budget firstPersonalBudget = firstUser.Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));
            Budget secondPersonalBudget = secondUser.Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));

            Budget familyBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Family,
                Currency = currency,
                Bank = bank,
                BudgetBalance = firstPersonalBudget.BudgetBalance + secondPersonalBudget.BudgetBalance,
                CreationDate = DateTime.Now,
            };

            familyBudget.Transactions.AddRange(firstPersonalBudget.Transactions);
            familyBudget.Transactions.AddRange(secondPersonalBudget.Transactions);

            repositoryWrapper.Budgets.Create(familyBudget);
            firstUser.Budgets.Add(familyBudget);
            secondUser.Budgets.Add(familyBudget);

            repositoryWrapper.Budgets.Delete(firstPersonalBudget);
            repositoryWrapper.Budgets.Delete(secondPersonalBudget);

            await repositoryWrapper.SaveChanges();
            return familyBudget;
        }
    }
}
