using FluentAssertions;
using Models.DTOs.Outgoing.Insights;
using Models.Entities;
using Models.Enums;
using Splitted_backend.EntitiesFilters;
using Splitted_backend.Managers;
using Splitted_backend.Utils;
using SplittedUnitTests.Data.FakeManagersData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ManagersTests.InsightsManagerTests
{
    public class InsightsManagerGetIncomeExpensesTests
    {
        private List<Transaction> transactions { get; }


        public InsightsManagerGetIncomeExpensesTests()
        {
            List<Transaction> newTransactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = new Guid("45cb7616-7440-49f8-b978-3d057ce0900c"),
                    Amount = -900,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-01"),
                    Description = "Transaction 23",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    UserCategory = "Clothes",
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("5b54fe55-e7ad-402d-a64a-e48455d8173c"),
                    Amount = 46,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-15"),
                    Description = "Transaction 24",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("5b54fe55-e7ad-402d-a64a-e48455d8173c"),
                    Amount = -84.6M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-30"),
                    Description = "Transaction 25",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("1725f663-6d91-42e9-9c84-8ba6b0987182"),
                    Amount = 500,
                    Currency = "PLN",
                    Date = DateTime.Parse("2024-01-10"),
                    Description = "Transaction 26",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("945692c7-fff1-472a-8e9d-63ea3294e818"),
                    Amount = -345.89M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2024-01-05"),
                    Description = "Transaction 27",
                    TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("40139cde-6a46-411e-9363-98fe740e9b20"),
                    Amount = 123,
                    Currency = "PLN",
                    Date = DateTime.Parse("2024-01-21"),
                    Description = "Transaction 28",
                    TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

            };

            transactions = new List<Transaction>(FakeTransactionsData.Transactions);
            transactions.AddRange(newTransactions);
        }


        [Fact]
        public void Test_GetIncomeExpenses()
        {
            InsightsIncomeExpensesDTO incomeExpensesDTO = InsightsManager.
                GetIncomeExpenses(FakeTransactionsData.Transactions);

            incomeExpensesDTO.Income.Should().Be(6172.34M);
            incomeExpensesDTO.Expenses.Should().Be(-5153.23M);
        }

        [Fact]
        public void Test_GetIncomeExpensesOverTimeByDay()
        {
            DateTime dateFrom = DateTime.Parse("2023-12-06");
            DateTime dateTo = DateTime.Parse("2023-12-22");

            List<InsightsIncomeExpensesOverTimeDTO> expectedIncomeExpensesOverTimeDTOs = new List<InsightsIncomeExpensesOverTimeDTO>
            {
                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-06",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-07",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-08",
                    Income = 2670,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-09",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-10",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-11",
                    Income = 0,
                    Expenses = -1902,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-12",
                    Income = 234.45M,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-13",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-14",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-15",
                    Income = 167.89M,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-16",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-17",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-18",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-19",
                    Income = 1900,
                    Expenses = -1900,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-20",
                    Income = 0,
                    Expenses = 0,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-21",
                    Income = 1200,
                    Expenses = -1200,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12-22",
                    Income = 0,
                    Expenses = 0,
                },
            };

            List<InsightsIncomeExpensesOverTimeDTO> incomeExpensesOverTimeDTOs = InsightsManager
                .GetIncomeExpensesOverTime(transactions, dateFrom, dateTo, InsightsDeltaTimeEnum.Day);

            incomeExpensesOverTimeDTOs.Should().BeEquivalentTo(expectedIncomeExpensesOverTimeDTOs);
        }

        [Fact]
        public void Test_GetIncomeExpensesOverTimeByMonth()
        {
            DateTime dateFrom = DateTime.Parse("2023-11-15");
            DateTime dateTo = DateTime.Parse("2024-02-11");

            TransactionsFilter transactionsFilter = new TransactionsFilter(
                    dates: (dateFrom, dateTo),
                    amounts: (null, null)
            );

            List<InsightsIncomeExpensesOverTimeDTO> expectedIncomeExpensesOverTimeDTOs = new List<InsightsIncomeExpensesOverTimeDTO>
            {
                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-11",
                    Expenses = -84.6M,
                    Income = 46,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2023-12",
                    Expenses = -5153.23M,
                    Income = 6172.34M,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2024-01",
                    Expenses = -345.89M,
                    Income = 623,
                },

                new InsightsIncomeExpensesOverTimeDTO
                {
                    Date = "2024-02",
                    Expenses = 0,
                    Income = 0,
                }
            };

            List<InsightsIncomeExpensesOverTimeDTO> incomeExpensesOverTimeDTOs = InsightsManager
                .GetIncomeExpensesOverTime(transactionsFilter.GetFilteredTransactions(transactions), 
                dateFrom, dateTo, InsightsDeltaTimeEnum.Month);

            incomeExpensesOverTimeDTOs.Should().BeEquivalentTo(expectedIncomeExpensesOverTimeDTOs);
        }
    }
}
