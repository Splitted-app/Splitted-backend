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
    public class InsightsManagerGetExpensesTests
    {
        private List<Transaction> transactions { get; }


        public InsightsManagerGetExpensesTests()
        {
            List<Transaction> newTransactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = new Guid("b16a8f3d-d6a4-489f-9e3e-d1ca65ce20ac"),
                    Amount = 900,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-30"),
                    Description = "Transaction 23",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("0c5ad09f-06bf-4823-b93a-d8b32a02de6d"),
                    Amount = -123.5M,
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
                    Amount = -45M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-08"),
                    Description = "Transaction 25",
                    UserCategory = string.Empty,
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },

                new Transaction
                {
                    Id = new Guid("818e4547-a836-4b2b-a596-4cb3137f02fb"),
                    Amount = -234,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-11-05"),
                    Description = "Transaction 26",
                    UserCategory = "   ",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                    UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                },
            };

            transactions = new List<Transaction>(FakeTransactionsData.Transactions);
            transactions.AddRange(newTransactions);
        }


        [Fact]
        public void Test_GetExpensesBreakdownByCategories()
        {
            TransactionsFilter transactionsFilter = new TransactionsFilter(
                dates: (null, null),
                amounts: (null, 0)
            );

            List<InsightsCategoryExpensesDTO> expectedCategoryExpensesDTOs = new List<InsightsCategoryExpensesDTO>
            {
                new InsightsCategoryExpensesDTO
                {
                    CategoryName = "Clothes",
                    Expenses = -3100
                },

                new InsightsCategoryExpensesDTO
                {
                    CategoryName = "Shopping",
                    Expenses = -2052
                },

                new InsightsCategoryExpensesDTO
                {
                    CategoryName = "Bus",
                    Expenses = -1.23M
                },

                new InsightsCategoryExpensesDTO
                {
                    CategoryName = "Uncategorized",
                    Expenses = -402.5M
                },
            };

            List<InsightsCategoryExpensesDTO> categoryExpensesDTOs = InsightsManager.
                GetExpensesBreakdownByCategories(transactionsFilter.GetFilteredTransactions(transactions));

            categoryExpensesDTOs.Should().BeEquivalentTo(expectedCategoryExpensesDTOs);
        }

        [Fact]
        public void Test_GetExpensesHistogram()
        {
            TransactionsFilter transactionsFilter = new TransactionsFilter(
                dates: (null, null),
                amounts: (null, 0)
            );

            List<InsightsExpensesHistogramDTO> expectedExpensesHistogramDTOs = new List<InsightsExpensesHistogramDTO>
            {
                new InsightsExpensesHistogramDTO
                {
                    Range = "0-99",
                    Count = 2,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "100-199",
                    Count = 2,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "200-299",
                    Count = 1,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "300-399",
                    Count = 0,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "400-499",
                    Count = 0,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "500-599",
                    Count = 0,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "600-699",
                    Count = 0,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "700-799",
                    Count = 0,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "800-899",
                    Count = 0,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "900-999",
                    Count = 1,
                },

                new InsightsExpensesHistogramDTO
                {
                    Range = "1000+",
                    Count = 3,
                },
            };

            List<InsightsExpensesHistogramDTO> expensesHistogramDTOs = InsightsManager.
                GetExpensesHistogram(transactionsFilter.GetFilteredTransactions(transactions), 100);

            expensesHistogramDTOs.Should().BeEquivalentTo(expectedExpensesHistogramDTOs);
        }

        [Fact]
        public void Test_GetExpensesSummary()
        {
            TransactionsFilter transactionsFilter = new TransactionsFilter(
                dates: (null, null),
                amounts: (null, 0)
            );

            InsightsSummaryDTO expectedSummaryDTO = new InsightsSummaryDTO 
            { 
                MaxValue = 1902,
                MinValue = 1.23M,
                Mean = 617.30333333333333333333333333M,
                Q1 = 123.5M,
                Median = 234,
                Q3 = 1000,
            };

            InsightsSummaryDTO summaryDTO = InsightsManager.
                GetExpensesSummary(transactionsFilter.GetFilteredTransactions(transactions));

            summaryDTO.Should().BeEquivalentTo(expectedSummaryDTO);
        }
    }
}
