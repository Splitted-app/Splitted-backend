using FluentAssertions;
using Models.DTOs.Outgoing.Insights;
using Models.Entities;
using Splitted_backend.EntitiesFilters;
using Splitted_backend.Managers;
using SplittedUnitTests.Data.FakeManagersData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ManagersTests.InsightsManagerTests
{
    public class InsightsManagerGetBalanceHistoryTests
    {
        private List<Transaction> transactions { get;}


        public InsightsManagerGetBalanceHistoryTests()
        {
            List<Transaction> newTransactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = new Guid("b16a8f3d-d6a4-489f-9e3e-d1ca65ce20ac"),
                    Amount = -900,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-30"),
                    Description = "Transaction 23",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    UserCategory = "Clothes",
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("0c5ad09f-06bf-4823-b93a-d8b32a02de6d"),
                    Amount = 123.5M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-15"),
                    Description = "Transaction 24",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("957ec8fc-23df-4395-bd5e-537b23bcf2a8"),
                    Amount = 45M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-08"),
                    Description = "Transaction 25",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("818e4547-a836-4b2b-a596-4cb3137f02fb"),
                    Amount = -134,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-05"),
                    Description = "Transaction 26",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },
            };

            transactions = new List<Transaction>(FakeBudgetsData.Budgets[5].Transactions);
            transactions.AddRange(newTransactions);
        }


        [Fact]
        public void Test_GetBalanceHistoryByDay()
        {
            DateTime dateFrom = DateTime.Parse("2023-11-29");
            DateTime dateTo = DateTime.Parse("2023-12-13");

            TransactionsFilter transactionsFilter = new TransactionsFilter(
                    dates: (dateFrom, null),
                    amounts: (null, null)
            );

            List<Transaction> transactionsFiltered = transactionsFilter.GetFilteredTransactions(transactions);
            DateTime lastTransactionDate = FakeBudgetsData.Budgets[5].Transactions.Max(t => t.Date);

            List<InsightsBalanceHistoryDTO> expectedBalanceHistoryDTOs = new List<InsightsBalanceHistoryDTO>
            {
                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-13",
                    Balance = 4100,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-12",
                    Balance = 4100,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-11",
                    Balance = 3865.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-10",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-09",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-08",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-07",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-06",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-05",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-04",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-03",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-02",
                    Balance = 5767.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12-01",
                    Balance = 5917.55M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-11-30",
                    Balance = 5918.78M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-11-29",
                    Balance = 6818.78M,
                }
            };

            List<InsightsBalanceHistoryDTO> balanceHistoryDTOs = InsightsManager
                .GetBalanceHistory(transactionsFiltered, dateFrom, dateTo, lastTransactionDate, 
                Models.Enums.InsightsDeltaTimeEnum.Day, FakeBudgetsData.Budgets[5].BudgetBalance);

            balanceHistoryDTOs.Should().BeEquivalentTo(expectedBalanceHistoryDTOs);
        }

        [Fact]
        public void Test_GetBalanceHistoryByMonth()
        {
            DateTime dateFrom = DateTime.Parse("2023-10-29");
            DateTime dateTo = DateTime.Parse("2023-12-20");

            TransactionsFilter transactionsFilter = new TransactionsFilter(
                    dates: (dateFrom, null),
                    amounts: (null, null)
            );

            List<Transaction> transactionsFiltered = transactionsFilter.GetFilteredTransactions(transactions);
            DateTime lastTransactionDate = FakeBudgetsData.Budgets[5].Transactions.Max(t => t.Date);

            List<InsightsBalanceHistoryDTO> expectedBalanceHistoryDTOs = new List<InsightsBalanceHistoryDTO>
            {
                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-10",
                    Balance = 6784.28M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-11",
                    Balance = 5918.78M,
                },

                new InsightsBalanceHistoryDTO
                {
                    Date = "2023-12",
                    Balance = 2200M,
                },
            };

            List<InsightsBalanceHistoryDTO> balanceHistoryDTOs = InsightsManager
                .GetBalanceHistory(transactionsFiltered, dateFrom, dateTo, lastTransactionDate,
                Models.Enums.InsightsDeltaTimeEnum.Month, FakeBudgetsData.Budgets[5].BudgetBalance);

            balanceHistoryDTOs.Should().BeEquivalentTo(expectedBalanceHistoryDTOs);
        }
    }
}
