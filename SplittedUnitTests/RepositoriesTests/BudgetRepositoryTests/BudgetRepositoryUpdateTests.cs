using FluentAssertions;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data;
using SplittedUnitTests.Data.FakeRepositoriesData;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.BudgetRepositoryTests
{

    public class BudgetRepositoryUpdateTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public BudgetRepositoryUpdateTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext(FakeBudgetsData.Budgets,
                FakeTransactionsData.Transactions, FakeGoalsData.Goals));
        }


        [Fact]
        public void Test_UpdateExistingBudget()
        {
            string name = "ChangedPartnerBudget";
            decimal budgetBalance = 15000;

            Budget budget = new Budget
            {
                Id = FakeBudgetsData.Budgets[2].Id,
                BudgetType = FakeBudgetsData.Budgets[2].BudgetType,
                Name = name,
                Currency = FakeBudgetsData.Budgets[2].Currency,
                BudgetBalance = budgetBalance,
                CreationDate = FakeBudgetsData.Budgets[2].CreationDate,
            };

            repositoryWrapper.Budgets.Update(budget);
            Budget? budgetFound = repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budget.Id));

            budgetFound.Should().NotBeNull();
            budgetFound.Should().BeEquivalentTo(budget);
        }

        [Fact]
        public void Test_UpdateNonExistingBudget()
        {
            string name = "ChangedPartnerBudget";
            decimal budgetBalance = 15000;

            Budget budget = new Budget
            {
                Id = new Guid("319d2454-86ff-4c3a-a248-e85ac8b22ee0"),
                BudgetType = Models.Enums.BudgetTypeEnum.Partner,
                Name = name,
                Currency = "PLN",
                BudgetBalance = budgetBalance,
                CreationDate = DateTime.Parse("2023-12-05"),
            };

            repositoryWrapper.Budgets.Update(budget);
            Budget? budgetFound = repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budget.Id));

            budgetFound.Should().BeNull();
        }
    }
}
