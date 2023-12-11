using Models.DTOs.Incoming.Goal;
using Models.DTOs.Outgoing.Goal;
using Models.DTOs.Outgoing.Insights;
using Models.Entities;
using Models.Enums;
using Splitted_backend.EntitiesFilters;

namespace Splitted_backend.Managers
{
    public static class GoalManager
    {
        private static Dictionary<GoalTypeEnum, string> GoalTypeToNameMapping { get; }
            = new Dictionary<GoalTypeEnum, string>
        {
            { GoalTypeEnum.AccountBalance, "Account balance" },
            { GoalTypeEnum.AverageExpenses, "Average expenses" },
            { GoalTypeEnum.ExpensesLimit, "Expenses limit" }
        };

        private static Dictionary<GoalTypeEnum, Func<GoalGetDTO, Budget, double>> GoalTypeToPercentagesMapping { get; }
            = new Dictionary<GoalTypeEnum, Func<GoalGetDTO, Budget, double>>
        {
            { GoalTypeEnum.AccountBalance, CountPercentageInBudgetBalance},
            { GoalTypeEnum.AverageExpenses, CountPercentageInAverageExpenses},
            { GoalTypeEnum.ExpensesLimit, CountPercentageInMaxExpenses}
        };


        public static void SetGoalName(GoalPostDTO goalPostDTO, Goal goal)
        {
            string name = GoalTypeToNameMapping[(GoalTypeEnum) goalPostDTO.GoalType!];
            if (goalPostDTO.Category is not null)
                name = string.Concat(name, $" in {goalPostDTO.Category}");

            goal.Name = name;
        }

        public static void CountPercentages(List<GoalGetDTO> goalGetDTOs, Budget budget)
        {
            goalGetDTOs.ForEach(g => g.Percentage = GoalTypeToPercentagesMapping[g.GoalType](g, budget));
        }

        public static double CountPercentageInBudgetBalance(GoalGetDTO goalGetDTO, Budget budget)
        {
            double percentage = decimal.ToDouble(budget.BudgetBalance / goalGetDTO.Amount * 100);
            percentage = percentage < 0 ? 0 : percentage;
            percentage = percentage > 100 ? 100 : percentage;

            return Math.Round(percentage, 2);
        }

        public static double CountPercentageInAverageExpenses(GoalGetDTO goalGetDTO, Budget budget)
        {
            TransactionsFilter transactionsFilter = new TransactionsFilter(
                dates: (goalGetDTO.CreationDate, DateTime.Today),
                amounts: (null, 0),
                category: goalGetDTO.Category
            );
            List<Transaction> filteredTransactions = transactionsFilter.GetFilteredTransactions(budget.Transactions);

            decimal expenses = InsightsManager.GetIncomeExpenses(filteredTransactions).Expenses;
            if (expenses == 0)
                return 100;

            int expensesNumber = filteredTransactions.Count;

            double ratio = Math.Abs(decimal.ToDouble(goalGetDTO.Amount * expensesNumber / expenses));
            double percentage = Math.Pow(ratio, 3) * 100;
            percentage = percentage > 100 ? 100 : percentage;

            return Math.Round(percentage, 2);
        }

        public static double CountPercentageInMaxExpenses(GoalGetDTO goalGetDTO, Budget budget)
        {
            TransactionsFilter transactionsFilter = new TransactionsFilter(
                dates: (goalGetDTO.CreationDate, DateTime.Today),
                amounts: (null, 0),
                category: goalGetDTO.Category
            );
            List<Transaction> filteredTransactions = transactionsFilter.GetFilteredTransactions(budget.Transactions);

            decimal expenses = InsightsManager.GetIncomeExpenses(filteredTransactions).Expenses;
            if (expenses == 0)
                return 100;

            int T = (goalGetDTO.Deadline - goalGetDTO.CreationDate).Days + 1;
            decimal expensesDailyGoal = goalGetDTO.Amount / T;

            int t =  (DateTime.Today - goalGetDTO.CreationDate).Days + 1;
            decimal expensesDailyActual = expenses / t;

            double b = (double)t / T;
            double ratio = Math.Abs(decimal.ToDouble(expensesDailyGoal / expensesDailyActual));
            double percentage = Math.Pow(ratio, 1 + b) * 100;
            percentage = percentage > 100 ? 100 : percentage;

            return Math.Round(percentage, 2);
        }
    }
}
