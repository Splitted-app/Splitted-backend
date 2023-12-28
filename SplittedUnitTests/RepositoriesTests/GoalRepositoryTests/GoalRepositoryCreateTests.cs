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

namespace SplittedUnitTests.RepositoriesTests.GoalRepositoryTests
{
    public class GoalRepositoryCreateTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public GoalRepositoryCreateTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext(FakeBudgetsData.Budgets,
                FakeTransactionsData.Transactions, FakeGoalsData.Goals));
        }


        [Fact]
        public void Test_CreateSingleGoal()
        {
            Goal goal = new Goal
            {
                Id = new Guid("89d53832-5fe1-4d52-bb39-e4ea47a7e49c"),
                Amount = 20000,
                Name = "Account balance",
                GoalType = Models.Enums.GoalTypeEnum.AccountBalance,
                CreationDate = DateTime.Parse("2023-12-01"),
                Deadline = DateTime.Parse("2023-12-31"),
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
            };

            repositoryWrapper.Goals.Create(goal);
            List<Goal> goalsFound = repositoryWrapper.Goals.GetAll();
            Goal? goalFound = repositoryWrapper.Goals.GetEntityOrDefaultByCondition(g => g.Id.Equals(goal.Id));

            goalsFound.Should().NotBeNull();
            goalsFound.Should().HaveCount(6);

            goalFound.Should().NotBeNull();
            goalFound.Should().BeEquivalentTo(goal);
        }

        [Fact]
        public async Task Test_CreateMultipleGoalsAsync()
        {
            List<Goal> goals = new List<Goal>
            {
                new Goal
                {
                    Id = new Guid("0064def7-3a55-4305-8a83-5e7c24eabafe"),
                    Amount = 40,
                    Name = "Average expenses in grocery",
                    GoalType = Models.Enums.GoalTypeEnum.AverageExpenses,
                    CreationDate = DateTime.Parse("2023-08-05"),
                    Deadline = DateTime.Parse("2023-08-31"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },

                new Goal
                {
                    Id = new Guid("2c4b11bf-0e33-41e1-9fd0-e99d98628d0a"),
                    Amount = 15000,
                    Name = "Account balance",
                    GoalType = Models.Enums.GoalTypeEnum.AccountBalance,
                    CreationDate = DateTime.Parse("2023-09-10"),
                    Deadline = DateTime.Parse("2023-10-10"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },

                new Goal
                {
                    Id = new Guid("30cb9f7e-64d4-4c73-960c-5f986fc10291"),
                    Amount = 4300,
                    Name = "Expenses limit in shopping",
                    GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                    CreationDate = DateTime.Parse("2023-12-30"),
                    Deadline = DateTime.Parse("2024-01-20"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },
            };

            await repositoryWrapper.Goals.CreateMultipleAsync(goals);
            List<Goal> goalsFound = repositoryWrapper.Goals.GetAll();

            goalsFound.Should().NotBeNull();
            goalsFound.Should().HaveCount(8);
        }
    }
}
