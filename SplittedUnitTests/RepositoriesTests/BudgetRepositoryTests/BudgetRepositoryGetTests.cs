using FluentAssertions;
using Models.Entities;
using Models.Enums;
using Splitted_backend.DbContexts;
using Splitted_backend.Repositories;
using SplittedUnitTests.RepositoriesTests.Fixtures;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace SplittedUnitTests.RepositoriesTests.BudgetRepositoryTests
{
    public class BudgetRepositoryGetTests : IClassFixture<RepositoryGetTestFixture>
    {
        private RepositoryGetTestFixture repositoryGetTestFixture { get; }


        public BudgetRepositoryGetTests(RepositoryGetTestFixture repositoryGetTestFixture)
        {
            this.repositoryGetTestFixture = repositoryGetTestFixture;
        }


        [Fact]
        public void Test_GetExistingBudgetById()
        {
            Guid id = new Guid("a618304c-ad79-452a-ba6a-f54828f8a249");

            Budget? budget = repositoryGetTestFixture.repositoryWrapper.Budgets
                .GetEntityOrDefaultByCondition(b => b.Id.Equals(id));

            budget.Should().NotBeNull();
            budget.Should().BeEquivalentTo(new Budget
            {
                Id = new Guid("a618304c-ad79-452a-ba6a-f54828f8a249"),
                Bank = BankNameEnum.Ing,
                BudgetType = BudgetTypeEnum.Family,
                Name = "FamilyBudget",
                Currency = "PLN",
                BudgetBalance = -100,
                CreationDate = DateTime.Parse("2023-11-12")
            });
        }

        [Fact]
        public void Test_GetNonExistingBudgetById()
        {
            Guid id = new Guid("bda3eec3-fc62-4bc5-aaea-1dd8cf4c1bc4");

            Budget? budget = repositoryGetTestFixture.repositoryWrapper.Budgets
                .GetEntityOrDefaultByCondition(b => b.Id.Equals(id));

            budget.Should().BeNull();
        }

        [Fact]
        public async Task Test_GetExistingBudgetBydIdAsync()
        {
            Guid id = new Guid("5451431a-f252-43a4-bbf2-71508d95d563");

            Budget? budget = await repositoryGetTestFixture.repositoryWrapper.Budgets
                .GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(id));

            budget.Should().NotBeNull();
            budget.Should().BeEquivalentTo(new Budget
            {
                Id = new Guid("5451431a-f252-43a4-bbf2-71508d95d563"),
                BudgetType = BudgetTypeEnum.Partner,
                Name = "PartnerBudget",
                Currency = "PLN",
                BudgetBalance = 12000,
                CreationDate = DateTime.Parse("2023-10-12")
            });
        }

        [Fact]
        public async Task Test_GetNonExistingBudgetBydIdAsync()
        {
            Guid id = new Guid("652d0b48-e93f-4138-94ac-d9add3d6a2c5");

            Budget? budget = await repositoryGetTestFixture.repositoryWrapper.Budgets
                .GetEntityOrDefaultByConditionAsync(b => b.Id.Equals(id));

            budget.Should().BeNull();
        }

        [Fact]
        public async Task Test_GetAllBudgetsAsync()
        {
            List<Budget> budgets = await repositoryGetTestFixture.repositoryWrapper.Budgets.GetAllAsync();

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(5);
        }

        [Fact]
        public async Task Test_GetExistingBudgetsByBudgetTypeAsync()
        {
            BudgetTypeEnum budgetType = BudgetTypeEnum.Personal;
            List<Budget> expectedBudgets = new List<Budget>
            {
                new Budget
                {
                    Id = new Guid("5b2294c4-5ab5-4ad2-b943-e4a30feb609f"),
                    Bank = BankNameEnum.Pekao,
                    BudgetType = BudgetTypeEnum.Personal,
                    Currency = "PLN",
                    BudgetBalance = 1000,
                    CreationDate = DateTime.Parse("2023-12-12")
                },
                new Budget
                {
                    Id = new Guid("4d688676-44e1-4e76-bfbe-4e269df1eec4"),
                    Bank = BankNameEnum.Santander,
                    BudgetType = BudgetTypeEnum.Personal,
                    Currency = "PLN",
                    BudgetBalance = 13456.34M,
                    CreationDate = DateTime.Parse("2023-08-12")
                },
            };

            List<Budget> budgets = await repositoryGetTestFixture.repositoryWrapper.Budgets
                .GetEntitiesByConditionAsync(b => b.BudgetType.Equals(budgetType));
            
            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(2);
            budgets.Should().BeEquivalentTo(expectedBudgets);
        }

        [Fact]
        public async Task Test_GetNonExistingBudgetsByBudgetTypeAsync()
        {
            BudgetTypeEnum budgetType = BudgetTypeEnum.Temporary;
            
            List<Budget> budgets = await repositoryGetTestFixture.repositoryWrapper.Budgets
                .GetEntitiesByConditionAsync(b => b.BudgetType.Equals(budgetType));

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(0);
        }
    }
}
