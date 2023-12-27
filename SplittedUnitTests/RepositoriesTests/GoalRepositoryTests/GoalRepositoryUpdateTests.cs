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
    public class GoalRepositoryUpdateTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public GoalRepositoryUpdateTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext());
        }


        [Fact]
        public void Test_UpdateExistingGoal()
        {
            decimal amount = 35;
            DateTime deadline = DateTime.Parse("2023-12-30");

            Goal goal = new Goal
            {
                Id = FakeGoalsData.Goals[3].Id,
                Amount = amount,
                Name = FakeGoalsData.Goals[3].Name,
                GoalType = FakeGoalsData.Goals[3].GoalType,
                CreationDate = FakeGoalsData.Goals[3].CreationDate,
                Deadline = deadline,
                IsMain = FakeGoalsData.Goals[3].IsMain,
                UserId = FakeGoalsData.Goals[3].UserId
            };

            repositoryWrapper.Goals.Update(goal);
            Goal? goalFound = repositoryWrapper.Goals.GetEntityOrDefaultByCondition(g => g.Id.Equals(goal.Id));

            goalFound.Should().NotBeNull();
            goalFound.Should().BeEquivalentTo(goal);
        }

        [Fact]
        public void Test_UpdateNonExistingBudget()
        {
            decimal amount = 6500;
            DateTime deadline = DateTime.Parse("2023-11-30");

            Goal goal = new Goal
            {
                Id = new Guid("91c835f5-b5eb-4e07-98c5-255abd14c800"),
                Amount = amount,
                Name = "Expenses limit",
                GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                CreationDate = DateTime.Parse("2023-11-01"),
                Deadline = deadline,
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
            };

            repositoryWrapper.Goals.Update(goal);
            Goal? goalFound = repositoryWrapper.Goals.GetEntityOrDefaultByCondition(g=> g.Id.Equals(goal.Id));

            goalFound.Should().BeNull();
        }
    }
}
