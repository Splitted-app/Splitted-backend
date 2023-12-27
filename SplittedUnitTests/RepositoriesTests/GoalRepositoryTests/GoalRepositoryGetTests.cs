using FluentAssertions;
using Models.Entities;
using Models.Enums;
using SplittedUnitTests.RepositoriesTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.GoalRepositoryTests
{
    public class GoalRepositoryGetTests : IClassFixture<RepositoryGetTestFixture>
    {
        private RepositoryGetTestFixture repositoryGetTestFixture { get; }


        public GoalRepositoryGetTests(RepositoryGetTestFixture repositoryGetTestFixture)
        {
            this.repositoryGetTestFixture = repositoryGetTestFixture;
        }


        [Fact]
        public void Test_GetExistingGoalById()
        {
            Guid id = new Guid("12feb8ce-7947-4457-aff3-b669499dee47");

            Goal? goal = repositoryGetTestFixture.repositoryWrapper.Goals
                .GetEntityOrDefaultByCondition(g => g.Id.Equals(id));

            goal.Should().NotBeNull();
            goal.Should().BeEquivalentTo(new Goal
            {
                Id = new Guid("12feb8ce-7947-4457-aff3-b669499dee47"),
                Amount = 8500,
                Name = "Expenses limit",
                GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                CreationDate = DateTime.Parse("2023-11-01"),
                Deadline = DateTime.Parse("2023-12-31"),
                IsMain = true,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            });
        }

        [Fact]
        public void Test_GetNonExistingGoalById()
        {
            Guid id = new Guid("bda3eec3-fc62-4bc5-aaea-1dd8cf4c1bc4");

            Goal? goal = repositoryGetTestFixture.repositoryWrapper.Goals
                .GetEntityOrDefaultByCondition(g => g.Id.Equals(id));

            goal.Should().BeNull();
        }

        [Fact]
        public async Task Test_GetExistingGoalBydIdAsync()
        {
            Guid id = new Guid("e43a23a6-070e-4fc5-baac-35149e7a63b4");

            Goal? goal = await repositoryGetTestFixture.repositoryWrapper.Goals
                .GetEntityOrDefaultByConditionAsync(g => g.Id.Equals(id));

            goal.Should().NotBeNull();
            goal.Should().BeEquivalentTo(new Goal
            {
                Id = new Guid("e43a23a6-070e-4fc5-baac-35149e7a63b4"),
                Amount = 5000,
                Name = "Expenses limit",
                GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                CreationDate = DateTime.Parse("2023-12-01"),
                Deadline = DateTime.Parse("2023-12-31"),
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
            });
        }

        [Fact]
        public async Task Test_GetNonExistingGoalBydIdAsync()
        {
            Guid id = new Guid("652d0b48-e93f-4138-94ac-d9add3d6a2c5");

            Goal? goal = await repositoryGetTestFixture.repositoryWrapper.Goals
                .GetEntityOrDefaultByConditionAsync(g => g.Id.Equals(id));

            goal.Should().BeNull();
        }

        [Fact]
        public async Task Test_GetAllGoalsAsync()
        {
            List<Goal> goals = await repositoryGetTestFixture.repositoryWrapper.Goals.GetAllAsync();

            goals.Should().NotBeNull();
            goals.Should().HaveCount(5);
        }

        [Fact]
        public async Task Test_GetExistingGoalsByGoalTypeAsync()
        {
            GoalTypeEnum goalType = GoalTypeEnum.AverageExpenses;
            List<Goal> expectedGoals = new List<Goal>
            {
                new Goal
                {
                    Id = new Guid("00aa12d7-dcdb-4daf-a9d8-f5b2cd22bb1d"),
                    Amount = 50,
                    Name = "Average expenses",
                    GoalType = Models.Enums.GoalTypeEnum.AverageExpenses,
                    CreationDate = DateTime.Parse("2023-11-01"),
                    Deadline = DateTime.Parse("2023-11-30"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },

                new Goal
                {
                    Id = new Guid("42f2c0c1-2967-42dd-b059-21afaab04077"),
                    Amount = 30,
                    Name = "Average expenses in shopping",
                    GoalType = Models.Enums.GoalTypeEnum.AverageExpenses,
                    CreationDate = DateTime.Parse("2023-12-05"),
                    Deadline = DateTime.Parse("2023-12-22"),
                    IsMain = false,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
                },
            };

            List<Goal> goals = await repositoryGetTestFixture.repositoryWrapper.Goals
                .GetEntitiesByConditionAsync(g => g.GoalType.Equals(goalType));

            goals.Should().NotBeNull();
            goals.Should().HaveCount(2);
            goals.Should().BeEquivalentTo(expectedGoals);
        }

        [Fact]
        public async Task Test_GetNonExistingGoalsByGoalTypeAsync()
        {
            GoalTypeEnum goalType = GoalTypeEnum.AccountBalance;

            List<Goal> goals = await repositoryGetTestFixture.repositoryWrapper.Goals
                .GetEntitiesByConditionAsync(g => g.GoalType.Equals(goalType));

            goals.Should().NotBeNull();
            goals.Should().HaveCount(0);
        }
    }
}
