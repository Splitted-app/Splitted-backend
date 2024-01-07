using FluentAssertions;
using Models.DTOs.Incoming.Goal;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Managers;
using Splitted_backend.Utils.TimeProvider;
using SplittedUnitTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ManagersTests.GoalManagerTests
{
    public class GoalManagerNamesTests
    {
        [Theory]
        [InlineData(null, GoalTypeEnum.AccountBalance, "Account balance")]
        [InlineData(null, GoalTypeEnum.AverageExpenses, "Average expenses")]
        [InlineData(null, GoalTypeEnum.ExpensesLimit, "Expenses limit")]
        [InlineData("Shopping", GoalTypeEnum.AverageExpenses, "Average expenses in Shopping")]
        [InlineData("grocery", GoalTypeEnum.ExpensesLimit, "Expenses limit in grocery")]
        public void Test_SetGoalsNames(string? category, GoalTypeEnum goalType, string expectedName)
        {
            GoalPostDTO goalPostDTO = new GoalPostDTO()
            {
                Amount = 9000,
                Category = category,
                GoalType = goalType,
                Deadline = DateTime.Parse("2023-12-21")
            };

            Goal goal = new Goal
            {
                Id = new Guid("12feb8ce-7947-4457-aff3-b669499dee47"),
                Category = goalPostDTO.Category?.ToLower() ?? null,
                Amount = (decimal) goalPostDTO.Amount,
                GoalType = goalType,
                CreationDate = DateTime.Parse("2023-12-12"),
                Deadline = (DateTime) goalPostDTO.Deadline,
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            };

            GoalManager.SetGoalName(goalPostDTO, goal);

            goal.Name.Should().Be(expectedName);
        }
    }
}
