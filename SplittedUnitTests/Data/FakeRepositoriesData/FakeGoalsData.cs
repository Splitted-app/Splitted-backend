using Models.Entities;
using Splitted_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Data.FakeRepositoriesData
{
    public static class FakeGoalsData
    {
        public static List<Goal> Goals = new List<Goal>
        {
            new Goal
            {
                Id = new Guid("12feb8ce-7947-4457-aff3-b669499dee47"),
                Amount = 8500,
                Name = "Expenses limit",
                GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                CreationDate = DateTime.Parse("2023-11-01"),
                Deadline = DateTime.Parse("2023-12-31"),
                IsMain = true,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            },

            new Goal
            {
                Id = new Guid("00aa12d7-dcdb-4daf-a9d8-f5b2cd22bb1d"),
                Amount = 50,
                Name = "Average expenses",
                GoalType = Models.Enums.GoalTypeEnum.AverageExpenses,
                CreationDate = DateTime.Parse("2023-11-01"),
                Deadline = DateTime.Parse("2023-11-30"),
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
            },

            new Goal
            {
                Id = new Guid("e43a23a6-070e-4fc5-baac-35149e7a63b4"),
                Amount = 5000,
                Name = "Expenses limit",
                GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                CreationDate = DateTime.Parse("2023-12-01"),
                Deadline = DateTime.Parse("2023-12-31"),
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
            },

            new Goal
            {
                Id = new Guid("42f2c0c1-2967-42dd-b059-21afaab04077"),
                Amount = 30,
                Name = "Average expenses in shopping",
                GoalType = Models.Enums.GoalTypeEnum.AverageExpenses,
                CreationDate = DateTime.Parse("2023-12-05"),
                Deadline = DateTime.Parse("2023-12-22"),
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
            },

            new Goal
            {
                Id = new Guid("921bd613-edb5-429a-8d38-87285c53a714"),
                Amount = 3000,
                Name = "Expenses limit in shopping",
                GoalType = Models.Enums.GoalTypeEnum.ExpensesLimit,
                CreationDate = DateTime.Parse("2023-12-01"),
                Deadline = DateTime.Parse("2023-12-31"),
                IsMain = false,
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a")
            },
        };
    }
}
