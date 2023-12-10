using Models.DTOs.Incoming.Goal;
using Models.Entities;
using Models.Enums;

namespace Splitted_backend.Managers
{
    public static class GoalManager
    {
        private static Dictionary<GoalTypeEnum, string> GoalTypeToNameMapping { get; }
            = new Dictionary<GoalTypeEnum, string>
        {
            { GoalTypeEnum.BudgetBalance, "Account balance" },
            { GoalTypeEnum.AverageExpenses, "Average expenses" },
            { GoalTypeEnum.MaxExpenses, "Expenses limit" }
        };

        public static void SetGoalName(GoalPostDTO goalPostDTO, Goal goal)
        {
            string name = GoalTypeToNameMapping[(GoalTypeEnum) goalPostDTO.GoalType!];
            if (goalPostDTO.Category is not null)
                name = string.Concat(name, $" in {goalPostDTO.Category}");

            goal.Name = name;
        }
    }
}
