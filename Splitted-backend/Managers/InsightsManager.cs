using CsvConversion.Extensions;
using Microsoft.IdentityModel.Tokens;
using Models.DTOs.Outgoing.Insights;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Extensions;
using Splitted_backend.Utils;
using System.Runtime.InteropServices;

namespace Splitted_backend.Managers
{
    public static class InsightsManager
    {
        public static InsightsIncomeExpensesDTO GetIncomeExpenses(List<Transaction> transactions)
        {
            decimal income = transactions.Where(t => t.Amount > 0)
                            .Aggregate(0M, (cur, next) => cur + next.Amount);
            decimal expenses = transactions.Where(t => t.Amount < 0)
                            .Aggregate(0M, (cur, next) => cur + next.Amount);

            return new InsightsIncomeExpensesDTO
            {
                Income = income,
                Expenses = expenses,
            };
        }

        public static List<InsightsBalanceHistoryDTO> GetBalanceHistory(List<Transaction> transactions,
            DateTime? dateFrom, DateTime? dateTo, DateTime lastTransactionDate,
            InsightsDeltaTimeEnum deltaTime, decimal budgetBalance)
        {
            List<InsightsBalanceHistoryDTO> balanceHistoryDTOs = new List<InsightsBalanceHistoryDTO>();
            decimal balanceDifference = 0M;

            if (deltaTime.Equals(InsightsDeltaTimeEnum.Day))
            {
                List<IGrouping<DateTime, Transaction>> groupedTransactionsByDate = transactions
                    .GroupBy(t => t.Date)
                    .ToList();

                DateTime min = dateFrom is null ? groupedTransactionsByDate.Min(gt => gt.Key) : (DateTime)dateFrom;
                DateTime max = lastTransactionDate;
                dateTo = dateTo is null ? max : dateTo;

                List<DateTime> dates = DateTimeLoop.EachDay(min, max)
                    .OrderByDescending(d => d)
                    .ToList();

                dates.ForEach(d =>
                {
                    var groupedTransactions = groupedTransactionsByDate.FirstOrDefault(gt => gt.Key.Equals(d));

                    if (groupedTransactions is null)
                    {
                        if (d <= dateTo)
                        {
                            balanceHistoryDTOs.Add(new InsightsBalanceHistoryDTO
                            {
                                Date = d.ToString("yyyy-MM-dd"),
                                Balance = budgetBalance + balanceDifference,
                            });
                        }
                    }
                    else
                    {
                        List<Transaction> transactionsByDate = groupedTransactions.ToList();
                        if (d <= dateTo)
                        {
                            balanceHistoryDTOs.Add(new InsightsBalanceHistoryDTO
                            {
                                Date = d.ToString("yyyy-MM-dd"),
                                Balance = budgetBalance + balanceDifference,
                            });
                        }
                        
                        InsightsIncomeExpensesDTO incomeExpensesDTO = GetIncomeExpenses(transactionsByDate);
                        balanceDifference -= (incomeExpensesDTO.Expenses + incomeExpensesDTO.Income);
                    }
                    
                });
            }
            else
            {
                List<IGrouping<(int year, int month), Transaction>> groupedTransactionsByMonth = transactions
                    .GroupBy(t => (t.Date.Year, t.Date.Month))
                    .ToList();

                DateTime min = dateFrom is null ? groupedTransactionsByMonth
                    .Min(gt => new DateTime(gt.Key.year, gt.Key.month, 1))
                    : new DateTime(dateFrom.Value.Year, dateFrom.Value.Month, 1);
                DateTime max = lastTransactionDate;
                dateTo = dateTo is null ? max : dateTo;

                List<DateTime> dates = DateTimeLoop.EachMonth(min, max)
                    .OrderByDescending(d => d)
                    .ToList();
                dates.ForEach(d =>
                {
                    var groupedTransactions = groupedTransactionsByMonth.FirstOrDefault(gt => gt.Key.year.Equals(d.Year) &&
                        gt.Key.month.Equals(d.Month));

                    if (groupedTransactions is null)
                    {
                        if (d <= dateTo)
                        {
                            balanceHistoryDTOs.Add(new InsightsBalanceHistoryDTO
                            {
                                Date = $"{d.Year}-{d.Month.ToString("D2")}",
                                Balance = budgetBalance + balanceDifference,
                            });
                        }
                    }
                    else
                    {
                        
                        List<Transaction> transactionsByMonth = groupedTransactions.ToList();
                        if (d.Year == dateTo.Value.Year && d.Month == dateTo.Value.Month)
                        {
                            InsightsIncomeExpensesDTO incomeExpensesAfterDateTo = GetIncomeExpenses(transactionsByMonth
                                .Where(tbm => tbm.Date > dateTo)
                                .ToList());
                            balanceDifference -= (incomeExpensesAfterDateTo.Expenses + incomeExpensesAfterDateTo.Income);
                        }

                        if (d <= dateTo)
                        {
                            balanceHistoryDTOs.Add(new InsightsBalanceHistoryDTO
                            {
                                Date = $"{d.Year}-{d.Month.ToString("D2")}",
                                Balance = budgetBalance + balanceDifference,
                            });
                        }
                        
                        InsightsIncomeExpensesDTO incomeExpensesDTO = GetIncomeExpenses(transactionsByMonth
                            .Where(tbm =>
                            {
                                if (tbm.Date.Month == dateTo.Value.Month && tbm.Date.Year == dateTo.Value.Year)
                                    return tbm.Date <= dateTo;
                                else
                                    return true;
                            }
                            )
                            .ToList());
                        balanceDifference -= (incomeExpensesDTO.Expenses + incomeExpensesDTO.Income);
                    }
                });
            }

            return balanceHistoryDTOs.OrderBy(bh => bh.Date)
                .ToList();
        }

        public static List<InsightsCategoryExpensesDTO> GetExpensesBreakdownByCategories(List<Transaction> transactions)
        {
            HashSet<string> categories = new HashSet<string>();
            List<InsightsCategoryExpensesDTO> categoryExpensesDTOs = new List<InsightsCategoryExpensesDTO>();   

            transactions.ForEach(t => categories.Add(string.IsNullOrWhiteSpace(t.UserCategory) ? "uncategorized" 
                : t.UserCategory.ToLower().Beutify()));

            foreach (string category in categories)
            {
                List<Transaction> transactionFiltered = transactions.Where(t =>
                {
                    if (category.Equals("uncategorized"))
                    {
                        if (string.IsNullOrWhiteSpace(t.UserCategory)) return true;
                        else return false;
                    }
                    else
                    {
                        return category.Equals(t.UserCategory is null ? null : t.UserCategory.ToLower().Beutify());
                    }
                })
                .ToList();

                categoryExpensesDTOs.Add(new InsightsCategoryExpensesDTO
                {
                    CategoryName = category.FirstCharToUpper(),
                    Expenses = GetIncomeExpenses(transactionFiltered).Expenses,
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

                DateTime min = dateFrom is null ? groupedTransactionsByDate.Min(gt => gt.Key) : (DateTime)dateFrom;
                DateTime max = dateTo is null ? groupedTransactionsByDate.Max(gt => gt.Key) : (DateTime)dateTo;

                List <DateTime> dates = DateTimeLoop.EachDay(min, max)
                    .ToList();
                dates.ForEach(d =>
                {
                    var groupedTransactions = groupedTransactionsByDate.FirstOrDefault(gt => gt.Key.Equals(d));

                    if (groupedTransactions is null)
                    {
                        incomeExpensesOverTimeDTOs.Add(new InsightsIncomeExpensesOverTimeDTO
                        {
                            Date = d.ToString("yyyy-MM-dd"),
                            Expenses = 0,
                            Income = 0,
                        });
                    }
                    else
                    {
                        List<Transaction> transactionsByDate = groupedTransactions.ToList();
                        InsightsIncomeExpensesDTO incomeExpensesDTO = GetIncomeExpenses(transactionsByDate);

                        incomeExpensesOverTimeDTOs.Add(new InsightsIncomeExpensesOverTimeDTO
                        {
                            Date = d.ToString("yyyy-MM-dd"),
                            Expenses = incomeExpensesDTO.Expenses,
                            Income = incomeExpensesDTO.Income,
                        });
                    }
                });
            }
            else
            {
                List<IGrouping<(int year, int month), Transaction>> groupedTransactionsByMonth = transactions
                    .GroupBy(t => (t.Date.Year, t.Date.Month))
                    .ToList();

                DateTime min = dateFrom is null ? groupedTransactionsByMonth
                    .Min(gt => new DateTime(gt.Key.year, gt.Key.month, 1)) : (DateTime)dateFrom;
                DateTime max = dateTo is null ? groupedTransactionsByMonth
                    .Max(gt => new DateTime(gt.Key.year, gt.Key.month, 1)) : (DateTime)dateTo;

                List<DateTime> dates = DateTimeLoop.EachMonth(min, max)
                    .ToList();
                dates.ForEach(d =>
                {
                    var groupedTransactions = groupedTransactionsByMonth.FirstOrDefault(gt => gt.Key.year.Equals(d.Year) &&
                        gt.Key.month.Equals(d.Month));

                    if (groupedTransactions is null)
                    {
                        incomeExpensesOverTimeDTOs.Add(new InsightsIncomeExpensesOverTimeDTO
                        {
                            Date = $"{d.Year}-{d.Month.ToString("D2")}",
                            Expenses = 0,
                            Income = 0,
                        });
                    }
                    else
                    {
                        List<Transaction> transactionsByMonth = groupedTransactions.ToList();
                        InsightsIncomeExpensesDTO incomeExpensesDTO = GetIncomeExpenses(transactionsByMonth);

                        incomeExpensesOverTimeDTOs.Add(new InsightsIncomeExpensesOverTimeDTO
                        {
                            Date = $"{d.Year}-{d.Month.ToString("D2")}",
                            Expenses = incomeExpensesDTO.Expenses,
                            Income = incomeExpensesDTO.Income,
                        });
                    }
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
                MaxValue = transactionsAmounts.Count == 0 ? 0 : transactionsAmounts.Max(),
                MinValue = transactionsAmounts.Count == 0 ? 0 : transactionsAmounts.Min(),
                Mean = transactionsAmounts.Count == 0 ? 0 : transactionsAmounts.Average(),
                Q1 = transactionsAmounts.Count == 0 ? 0 : transactionsAmounts.Percentile(25M),
                Median = transactionsAmounts.Count == 0 ? 0 : transactionsAmounts.Percentile(50M),
                Q3 = transactionsAmounts.Count == 0 ? 0 : transactionsAmounts.Percentile(75M),
            };
        }
    }
}
