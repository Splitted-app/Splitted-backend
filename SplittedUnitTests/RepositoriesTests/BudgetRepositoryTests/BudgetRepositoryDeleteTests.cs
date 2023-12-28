using FluentAssertions;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data;
using SplittedUnitTests.Data.FakeRepositoriesData;
using SplittedUnitTests.RepositoriesTests.Mocks;

namespace SplittedUnitTests.RepositoriesTests.BudgetRepositoryTests
{
    public class BudgetRepositoryDeleteTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public BudgetRepositoryDeleteTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext(FakeBudgetsData.Budgets,
                FakeTransactionsData.Transactions, FakeGoalsData.Goals));
        }


        [Fact]
        public void Test_DeleteExistingBudget()
        {
            Budget budget = FakeBudgetsData.Budgets[1];

            repositoryWrapper.Budgets.Delete(budget);
            List<Budget> budgetsFound = repositoryWrapper.Budgets.GetAll();
            Budget? budgetFound = repositoryWrapper.Budgets.GetEntityOrDefaultByCondition(b => b.Id.Equals(budget.Id));

            budgetsFound.Should().NotBeNull();
            budgetsFound.Should().HaveCount(4);

            budgetFound.Should().BeNull();
        }

        [Fact]
        public void Test_DeleteNonExistingBudget()
        {
            Budget budget = new Budget
            {
                Id = new Guid("23ed1912-8612-4548-b4b1-88b2b4a1b2bd"),
                Bank = Models.Enums.BankNameEnum.Other,
                BudgetType = Models.Enums.BudgetTypeEnum.Family,
                Currency = "PLN",
                BudgetBalance = 12000.35M,
                CreationDate = DateTime.Parse("2023-08-15")
            };

            repositoryWrapper.Budgets.Delete(budget);
            List<Budget> budgetsFound = repositoryWrapper.Budgets.GetAll();

            budgetsFound.Should().NotBeNull();
            budgetsFound.Should().HaveCount(5);
        }

        [Fact]
        public void Test_DeleteExistingBudgets()
        {
            List<Budget> budgets = new List<Budget>
            {
                FakeBudgetsData.Budgets[0],
                FakeBudgetsData.Budgets[2],
                FakeBudgetsData.Budgets[4],
            };

            repositoryWrapper.Budgets.DeleteMultiple(budgets);
            List<Budget> budgetsFound = repositoryWrapper.Budgets.GetAll();

            budgetsFound.Should().NotBeNull();
            budgetsFound.Should().HaveCount(2);
        }

        [Fact]
        public void Test_DeleteNonExistingBudgets()
        {
            List<Budget> budgets = new List<Budget>
            {
                new Budget
                {
                    Id = new Guid("c955b301-3a3d-4489-a27f-d1808e3abaf3"),
                    Bank = Models.Enums.BankNameEnum.Other,
                    BudgetType = Models.Enums.BudgetTypeEnum.Family,
                    Currency = "EUR",
                    BudgetBalance = 25300M,
                    CreationDate = DateTime.Parse("2023-11-21")
                },

                new Budget
                {
                    Id = new Guid("6c6693a7-bce4-45b5-8f94-b3a79ad12a4b"),
                    Bank = Models.Enums.BankNameEnum.Santander,
                    BudgetType = Models.Enums.BudgetTypeEnum.Temporary,
                    Currency = "PLN",
                    BudgetBalance = 19005M,
                    CreationDate = DateTime.Parse("2023-11-19")
                },

                new Budget
                {
                    Id = new Guid("7a90a2db-603f-4c94-95e1-636c3a60acb5"),
                    Bank = Models.Enums.BankNameEnum.Pko,
                    BudgetType = Models.Enums.BudgetTypeEnum.Partner,
                    Currency = "PLN",
                    BudgetBalance = 29000.25M,
                    CreationDate = DateTime.Parse("2023-11-18")
                }
            };

            repositoryWrapper.Budgets.DeleteMultiple(budgets);
            List<Budget> budgetsFound = repositoryWrapper.Budgets.GetAll();

            budgetsFound.Should().NotBeNull();
            budgetsFound.Should().HaveCount(5);
        }

        [Fact]
        public void Test_DeleteExistingAndNonExistingBudgets()
        {
            List<Budget> budgets = new List<Budget>
            {
                FakeBudgetsData.Budgets[1],
                new Budget
                {
                    Id = new Guid("c955b301-3a3d-4489-a27f-d1808e3abaf3"),
                    Bank = Models.Enums.BankNameEnum.Other,
                    BudgetType = Models.Enums.BudgetTypeEnum.Family,
                    Currency = "EUR",
                    BudgetBalance = 25300M,
                    CreationDate = DateTime.Parse("2023-11-21")
                },
            };

            repositoryWrapper.Budgets.DeleteMultiple(budgets);
            List<Budget> budgetsFound = repositoryWrapper.Budgets.GetAll();
            Budget? budgetFound = repositoryWrapper.Budgets
                .GetEntityOrDefaultByCondition(b => b.Id.Equals(budgets[0].Id));

            budgetsFound.Should().NotBeNull();
            budgetsFound.Should().HaveCount(4);

            budgetFound.Should().BeNull();
        }
    }
}
