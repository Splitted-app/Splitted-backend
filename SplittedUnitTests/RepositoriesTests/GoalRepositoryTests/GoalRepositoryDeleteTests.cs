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
    public class GoalRepositoryDeleteTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public GoalRepositoryDeleteTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext(FakeBudgetsData.Budgets,
                FakeTransactionsData.Transactions, FakeGoalsData.Goals));
        }


        [Fact]
        public void Test_DeleteExistingGoal()
        {
            Goal goal = FakeGoalsData.Goals[3];

            repositoryWrapper.Goals.Delete(goal);
            List<Goal> goalsFound = repositoryWrapper.Goals.GetAll();
            Goal? goalFound = repositoryWrapper.Goals.GetEntityOrDefaultByCondition(g => g.Id.Equals(goal.Id));

            goalsFound.Should().NotBeNull();
            goalsFound.Should().HaveCount(4);

            goalFound.Should().BeNull();
        }

        [Fact]
        public void Test_DeleteNonExistingGoal()
        {
            Goal goal = new Goal
            {
                Id = new Guid("3b11192e-6509-4499-b870-c42706f15144"),
                Amount = 10000,
                Name = "Expenses limit in furniture",
                GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                CreationDate = DateTime.Parse("2023-12-15"),
                Deadline = DateTime.Parse("2024-02-29"),
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
            };

            repositoryWrapper.Goals.Delete(goal);
            List<Goal> goalsFound = repositoryWrapper.Goals.GetAll();

            goalsFound.Should().NotBeNull();
            goalsFound.Should().HaveCount(5);
        }

        [Fact]
        public void Test_DeleteExistingGoals()
        {
            List<Goal> goals = new List<Goal>
            {
                FakeGoalsData.Goals[0],
                FakeGoalsData.Goals[2],
                FakeGoalsData.Goals[4],
            };

            repositoryWrapper.Goals.DeleteMultiple(goals);
            List<Goal> goalsFound = repositoryWrapper.Goals.GetAll();

            goalsFound.Should().NotBeNull();
            goalsFound.Should().HaveCount(2);
        }

        [Fact]
        public void Test_DeleteNonExistingGoals()
        {
            List<Goal> goals = new List<Goal>
            {
                new Goal
                {
                    Id = new Guid("32ae2957-689c-49f3-a150-14a82bae417c"),
                    Amount = 1000,
                    Name = "Expenses limit in shopping",
                    GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                    CreationDate = DateTime.Parse("2023-12-01"),
                    Deadline = DateTime.Parse("2023-12-10"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },

                new Goal
                {
                    Id = new Guid("f70da289-b4c4-4ce6-ba50-208c4f016e39"),
                    Amount = 17500,
                    Name = "Account balance",
                    GoalType = Models.Enums.GoalTypeEnum.AccountBalance,
                    CreationDate = DateTime.Parse("2023-12-01"),
                    Deadline = DateTime.Parse("2023-01-10"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },

                new Goal
                {
                    Id = new Guid("e5089323-f028-455e-91e7-414c10d4302f"),
                    Amount = 75,
                    Name = "Average expenses",
                    GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                    CreationDate = DateTime.Parse("2023-12-01"),
                    Deadline = DateTime.Parse("2024-01-21"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },
            };

            repositoryWrapper.Goals.DeleteMultiple(goals);
            List<Goal> goalsFound = repositoryWrapper.Goals.GetAll();

            goalsFound.Should().NotBeNull();
            goalsFound.Should().HaveCount(5);
        }

        [Fact]
        public void Test_DeleteExistingAndNonExistingGoals()
        {
            List<Goal> goals = new List<Goal>
            {
                FakeGoalsData.Goals[2],
                new Goal
                {
                    Id = new Guid("62083c90-65c8-43b5-9a7c-2569fc75c623"),
                    Amount = 30,
                    Name = "Average expenses in shopping",
                    GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                    CreationDate = DateTime.Parse("2023-11-01"),
                    Deadline = DateTime.Parse("2024-11-21"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },
            };

            repositoryWrapper.Goals.DeleteMultiple(goals);
            List<Goal> goalsFound = repositoryWrapper.Goals.GetAll();
            Goal? goalFound = repositoryWrapper.Goals
                .GetEntityOrDefaultByCondition(g => g.Id.Equals(goals[0].Id));

            goalsFound.Should().NotBeNull();
            goalsFound.Should().HaveCount(4);

            goalFound.Should().BeNull();
        }
    }
}
