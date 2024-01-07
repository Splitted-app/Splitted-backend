using FluentAssertions;
using Models.DTOs.Outgoing.Goal;
using Models.Entities;
using Splitted_backend.Managers;
using SplittedUnitTests.Data.FakeManagersData;
using SplittedUnitTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ManagersTests.GoalManagerTests
{
    public class GoalManagerCountPercentagesTests
    {
        [Theory]
        [InlineData(8500, 0, 11.76)]
        [InlineData(800, 0, 100)]
        [InlineData(700, 7, 0)]
        public void Test_CountPercentageInBudgetBalanceGoal(decimal amount, int budgetIndex, double expectedPercentage)
        {
            List<GoalGetDTO> goals = new List<GoalGetDTO>
            {
                new GoalGetDTO
                {
                    Id = new Guid("e43a23a6-070e-4fc5-baac-35149e7a63b4"),
                    Amount = amount,
                    Name = "Account balance",
                    GoalType = Models.Enums.GoalTypeEnum.AccountBalance,
                    CreationDate = DateTime.Parse("2023-12-21"),
                    Deadline = DateTime.Parse("2023-12-31"),
                    IsMain = false,
                }
            };

            Budget budget = FakeBudgetsData.Budgets[budgetIndex];

            GoalManager.CountPercentages(goals, budget, 
                TimeProviderMock.GetMockedTimeProvider(DateTime.Parse("2023-12-21")));

            goals[0].Percentage.Should().Be(expectedPercentage);
        }

        [Theory]
        [InlineData(900, null, "2023-12-02", "2023-12-18", 67.50)]
        [InlineData(850.5, null, "2023-12-19", "2023-12-22", 55.76)]
        [InlineData(1200, null, "2023-12-19", "2023-12-22", 100)]
        [InlineData(1000, null, "2023-12-23", "2023-12-28", 100)]
        [InlineData(800, "shopping", "2023-12-02", "2023-12-28", 47.41)]
        [InlineData(1.10, "bus", "2023-11-30", "2023-12-12", 71.53)]
        [InlineData(15, "useless", "2023-12-19", "2023-12-31", 100)]
        public void Test_CountPercentageInAverageExpensesGoal(decimal amount, string? category,
            string creationDate, string today, double expectedPercentage)
        {
            List<GoalGetDTO> goals = new List<GoalGetDTO>
            {
                new GoalGetDTO
                {
                    Id = new Guid("3ebf1cc3-199d-40b0-8266-184b96ca4834"),
                    Amount = amount,
                    Name = "Average expenses",
                    Category = category,
                    GoalType = Models.Enums.GoalTypeEnum.AverageExpenses,
                    CreationDate = DateTime.Parse(creationDate),
                    Deadline = DateTime.Parse("2023-12-31"),
                    IsMain = false,
                }
            };

            Budget budget = FakeBudgetsData.Budgets[5];

            GoalManager.CountPercentages(goals, budget,
                TimeProviderMock.GetMockedTimeProvider(DateTime.Parse(today)));

            goals[0].Percentage.Should().Be(expectedPercentage);
        }

        [Theory]
        [InlineData(2500, null, "2023-12-02", "2023-12-18", "2024-01-10", 39.14)]
        [InlineData(5000, null, "2023-12-19", "2023-12-22", "2023-12-31", 40)]
        [InlineData(3200, null, "2023-12-19", "2023-12-22", "2023-12-22", 100)]
        [InlineData(1000, null, "2023-12-23", "2023-12-28", "2023-12-18", 100)]
        [InlineData(3000, "shopping", "2023-12-02", "2023-12-28", "2024-01-15", 81.09)]
        [InlineData(10, "bus", "2023-11-30", "2023-12-01", "2023-12-18", 84.19)]
        [InlineData(100, "useless", "2023-12-19", "2023-12-31", "2023-01-18", 100)]
        public void Test_CountPercentageInMaxExpensesGoal(decimal amount, string? category,
            string creationDate, string today, string deadline, double expectedPercentage)
        {
            List<GoalGetDTO> goals = new List<GoalGetDTO>
            {
                new GoalGetDTO
                {
                    Id = new Guid("74b28c7a-010a-4ada-b507-64dacc4a76bd"),
                    Amount = amount,
                    Name = "Expenses limit",
                    Category = category,
                    GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                    CreationDate = DateTime.Parse(creationDate),
                    Deadline = DateTime.Parse(deadline),
                    IsMain = false,
                }
            };

            Budget budget = FakeBudgetsData.Budgets[5];

            GoalManager.CountPercentages(goals, budget,
                TimeProviderMock.GetMockedTimeProvider(DateTime.Parse(today)));

            goals[0].Percentage.Should().Be(expectedPercentage);
        }
    }
}
