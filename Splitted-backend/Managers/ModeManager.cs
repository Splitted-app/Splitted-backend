using Microsoft.AspNetCore.Identity;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Incoming.Mode;
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
            User secondUser, FamilyModePostDTO familyModePostDTO)
        {
            Budget firstPersonalBudget = firstUser.Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));
            Budget secondPersonalBudget = secondUser.Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));

            Budget familyBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Family,
                Currency = familyModePostDTO.Currency,
                Bank = familyModePostDTO.Bank,
                BudgetBalance = firstPersonalBudget.BudgetBalance + secondPersonalBudget.BudgetBalance,
                CreationDate = DateTime.Now,
            };

            repositoryWrapper.Transactions.FindDuplicates(secondPersonalBudget.Transactions, firstPersonalBudget.Transactions);
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

        public static async Task<Budget> CreatePartnerMode(IRepositoryWrapper repositoryWrapper, User firstUser, 
            User secondUser, BudgetPostDTO budgetPostDTO)
        {
            Budget partnerBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Partner,
                Currency = budgetPostDTO.Currency,
                Bank = budgetPostDTO.Bank,
                BudgetBalance = 0,
                CreationDate = DateTime.Now,
            };

            repositoryWrapper.Budgets.Create(partnerBudget);
            firstUser.Budgets.Add(partnerBudget);
            secondUser.Budgets.Add(partnerBudget);

            await repositoryWrapper.SaveChanges();
            return partnerBudget;
        }
    }
}
