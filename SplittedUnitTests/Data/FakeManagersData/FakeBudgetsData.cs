using Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Data.FakeManagersData
{
    public class FakeBudgetsData
    {
        public static List<Budget> Budgets = new List<Budget>
        {
            new Budget
            {
                Id = new Guid("981463e1-2261-4586-b745-cc9944761f71"),
                Bank = Models.Enums.BankNameEnum.Pekao,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 1000,
                CreationDate = DateTime.Parse("2023-12-12")
            },

            new Budget
            {
                Id = new Guid("98cd1dee-274c-405e-aa82-b2147ab5bc13"),
                Bank = Models.Enums.BankNameEnum.Ing,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 1200,
                CreationDate = DateTime.Parse("2023-12-13")
            },

            new Budget
            {
                Id = new Guid("9ffafd1b-4b2b-426d-8dd9-e2634df28c75"),
                Bank = Models.Enums.BankNameEnum.Pekao,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 10000,
                CreationDate = DateTime.Parse("2023-12-12")
            },

            new Budget
            {
                Id = new Guid("06365522-6b68-4da3-b8e5-11164d27551e"),
                Bank = Models.Enums.BankNameEnum.Mbank,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 100,
                CreationDate = DateTime.Parse("2023-12-14")
            },

            new Budget
            {
                Id = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                Bank = Models.Enums.BankNameEnum.Pekao,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 12000,
                CreationDate = DateTime.Parse("2023-12-16")
            },

            new Budget
            {
                Id = new Guid("fd344234-576d-4907-b95e-7f1a21e5f6e2"),
                Bank = Models.Enums.BankNameEnum.Pekao,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 1000,
                CreationDate = DateTime.Parse("2023-12-17")
            },

            new Budget
            {
                Id = new Guid("ee05e5ae-597d-444c-b789-b31c6629b1c1"),
                Bank = Models.Enums.BankNameEnum.Pko,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 19200,
                CreationDate = DateTime.Parse("2023-12-12")
            },

            new Budget
            {
                Id = new Guid("3a2fd0e5-1970-4843-8b95-500d6145bab2"),
                Bank = Models.Enums.BankNameEnum.Santander,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 1000,
                CreationDate = DateTime.Parse("2023-12-17")
            },

            new Budget
            {
                Id = new Guid("23054de3-c8e8-4555-b47f-c74e310d82ad"),
                Bank = Models.Enums.BankNameEnum.Other,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 8800,
                CreationDate = DateTime.Parse("2023-12-20")
            },

            new Budget
            {
                Id = new Guid("29ea350d-8ab6-4fe6-87c5-30387f5c6182"),
                Bank = Models.Enums.BankNameEnum.Pko,
                BudgetType = Models.Enums.BudgetTypeEnum.Personal,
                Currency = "PLN",
                BudgetBalance = 1000,
                CreationDate = DateTime.Parse("2023-12-21")
            },
        };
    }
}
