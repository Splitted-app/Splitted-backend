using CsvHelper.TypeConversion;
using Models.DTOs.Outgoing.Insights;
using Models.Entities;
using Models.Enums;
using Org.BouncyCastle.Utilities.Collections;
using Splitted_backend.EntitiesFilters;
using Splitted_backend.Extensions;

namespace Splitted_backend.Managers
{
    public static class InsightsManager
    {
        public static (decimal income, decimal expenses) GetIncomeExpenses(List<Transaction> transactions)
        {
            decimal income = transactions.Where(t => t.Amount > 0)
                            .Aggregate(0M, (cur, next) => cur + next.Amount);
            decimal expenses = transactions.Where(t => t.Amount < 0)
                            .Aggregate(0M, (cur, next) => cur + next.Amount);

            return (income, expenses);
        }

        public static List<InsightsCategoryExpensesDTO> GetExpensesBreakdownByCategories(List<Transaction> transactions)
        {
            HashSet<string?> categories = new HashSet<string?>();
            List<InsightsCategoryExpensesDTO> categoryExpensesDTOs = new List<InsightsCategoryExpensesDTO>();   

            transactions.ForEach(t => categories.Add(t.UserCategory));
            foreach (string? category in categories)
            {
                List<Transaction> transactionFiltered = transactions.Where(t =>
                {
                    if (category is null)
                    {
                        if (t.UserCategory is null) return true;
                        else return false;
                    }
                    else
                    {
                        return category.Equals(t.UserCategory);
                    }
                })
                .ToList();

                categoryExpensesDTOs.Add(new InsightsCategoryExpensesDTO
                {
                    CategoryName = category,
                    Expenses = GetIncomeExpenses(transactionFiltered).expenses,
                });
            }

            return categoryExpensesDTOs;
        }

        public static List<InsightsIncomeExpensesOverTimeDTO> GetIncomeExpensesOverTime(List<Transaction> transactions, 
            DateTime? dateFrom, DateTime? dateTo, InsightsDeltaTimeEnum deltaTime)
        {
            List<InsightsIncomeExpensesOverTimeDTO> incomeExpensesOverTimeDTOs = new List<InsightsIncomeExpensesOverTimeDTO>();

            if (deltaTime.Equals(InsightsDeltaTimeEnum.Day))
            {
                List<IGrouping<DateTime, Transaction>> groupedTransactionsByDate = transactions
                    .GroupBy(t => t.Date)
                    .ToList();

                groupedTransactionsByDate.ForEach(gt =>
                {
                    List<Transaction> transactionsByDate = gt.ToList();
                    (decimal income, decimal expenses) = GetIncomeExpenses(transactionsByDate);

                    incomeExpensesOverTimeDTOs.Add(new InsightsIncomeExpensesOverTimeDTO
                    {
                        Date = gt.Key.ToString(),
                        Expenses = expenses,
                        Income = income,
                    });
                });
            }
            else
            {
                List<IGrouping<(int month, int year), Transaction>> groupedTransactionByMonth = transactions
                    .GroupBy(t => (t.Date.Month, t.Date.Year))
                    .ToList();

                groupedTransactionByMonth.ForEach(gt =>
                {
                    List<Transaction> transactionsByDate = gt.ToList();
                    (decimal income, decimal expenses) = GetIncomeExpenses(transactionsByDate);

                    incomeExpensesOverTimeDTOs.Add(new InsightsIncomeExpensesOverTimeDTO
                    {
                        Date = $"{gt.Key.year}-{gt.Key.month.ToString("D2")}",
                        Expenses = expenses,
                        Income = income,
                    });
                });
            }

            return incomeExpensesOverTimeDTOs;
        }

        public static List<InsightsExpensesHistogramDTO> GetExpensesHistogram(List<Transaction> transactions, int binRange)
        {
            int binsNumber = 11;
            List<InsightsExpensesHistogramDTO> expensesHistogramDTOs = Enumerable.Range(0, binsNumber)
                .Select(i => new InsightsExpensesHistogramDTO
                {
                    Range = i < binsNumber - 1 ? $"{i * binRange}-{(i + 1) * binRange - 1}" : $"{i * binRange}+",
                    Count = 0,
                })
                .ToList();

            transactions.ForEach(t =>
            {
                int index = (int)Math.Abs(t.Amount) / binRange;
                if (index >= binsNumber)
                    index = binsNumber - 1;
                expensesHistogramDTOs[index].Count++;
            });
            
            return expensesHistogramDTOs;
        }

        public static InsightsSummaryDTO GetExpensesSummary(List<Transaction> transactions)
        {
            List<decimal> transactionsAmounts = transactions.Select(t => Math.Abs(t.Amount))
                .ToList();
            transactionsAmounts.Sort();

            return new InsightsSummaryDTO
            {
                MaxValue = transactionsAmounts.Max(),
                MinValue = transactionsAmounts.Min(),
                Mean = transactionsAmounts.Average(),
                Q1 = transactionsAmounts.Percentile(25M),
                Median = transactionsAmounts.Percentile(50M),
                Q3 = transactionsAmounts.Percentile(75M),
            };
        }
    }
}
