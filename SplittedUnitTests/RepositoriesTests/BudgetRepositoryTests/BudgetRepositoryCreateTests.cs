using FluentAssertions;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data;
using SplittedUnitTests.Data.FakeRepositoriesData;
using SplittedUnitTests.RepositoriesTests.Fixtures;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.BudgetRepositoryTests
{
    public class BudgetRepositoryCreateTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public BudgetRepositoryCreateTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext(FakeBudgetsData.Budgets,
                FakeTransactionsData.Transactions, FakeGoalsData.Goals));
        }


        [Fact]
        public void Test_CreateSingleBudget()
        {
            Budget budget = new Budget
            {
                Id = new Guid("2d7e6fac-a32b-4963-82f8-a08317c1fe14"),
                Bank = Models.Enums.BankNameEnum.Mbank,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 2000M,
                CreationDate = DateTime.Parse("2023-12-22")
            };

            repositoryWrapper.Budgets.Create(budget);
            List<Budget> budgetsFound = repositoryWrapper.Budgets.GetAll();
            Budget? budgetFound = repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budget.Id));

            budgetsFound.Should().NotBeNull();
            budgetsFound.Should().HaveCount(6);

            budgetFound.Should().NotBeNull();
            budgetFound.Should().BeEquivalentTo(budget);
        }

        [Fact]
        public async Task Test_CreateMultipleBudgetsAsync()
        {
            List<Budget> budgets = new List<Budget>
            {
                new Budget
                {
                    Id = new Guid("1936f63f-359e-4eac-bc7b-6b377ec992d6"),
                    Bank = Models.Enums.BankNameEnum.Other,
                    BudgetType = Models.Enums.BudgetTypeEnum.Family,
                    Currency = "EUR",
                    BudgetBalance = 2300M,
                    CreationDate = DateTime.Parse("2023-12-20")
                },
                
                new Budget
                {
                    Id = new Guid("4fc796f0-f20b-416a-a1e6-64e1ab4d0ae0"),
                    Bank = Models.Enums.BankNameEnum.Santander,
                    BudgetType = Models.Enums.BudgetTypeEnum.Temporary,
                    Currency = "PLN",
                    BudgetBalance = 13300M,
                    CreationDate = DateTime.Parse("2023-12-19")
                },

                new Budget
                {
                    Id = new Guid("f8d3d6f8-8b94-481f-8554-4774dc583f60"),
                    Bank = Models.Enums.BankNameEnum.Pko,
                    BudgetType = Models.Enums.BudgetTypeEnum.Partner,
                    Currency = "PLN",
                    BudgetBalance = 16567.25M,
                    CreationDate = DateTime.Parse("2023-12-18")
                }
            };

            await repositoryWrapper.Budgets.CreateMultipleAsync(budgets);
            List<Budget> budgetsFound = repositoryWrapper.Budgets.GetAll();

            budgetsFound.Should().NotBeNull();
            budgetsFound.Should().HaveCount(8);
        }
    }
}
