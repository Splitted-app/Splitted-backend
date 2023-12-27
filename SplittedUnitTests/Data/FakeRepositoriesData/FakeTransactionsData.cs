using Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Data.FakeRepositoriesData
{
    public static class FakeTransactionsData
    {
        public static List<Transaction> Transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = new Guid("224ac6a9-7699-4d25-91de-7244abb04459"),
                Amount = -150,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-02"),
                Description = "Transaction 1",
                TransactionType = Models.Enums.TransactionTypeEnum.Card,
                UserCategory = "Shopping",
                BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            },

            new Transaction
            {
                Id = new Guid("3dba0f55-f0a4-4b92-9c71-ac0408fcfca4"),
                Amount = 234.45M,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-12"),
                Description = "Transaction 2",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "Clothes",
                BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
            },

            new Transaction
            {
                Id = new Guid("a0a5b246-c369-4c45-a622-5b5f72dbc97e"),
                Amount = -1902,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-11"),
                Description = "Transaction 3",
                TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                UserCategory = "Shopping",
                BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            },

            new Transaction
            {
                Id = new Guid("a7ce392f-923b-491f-9fc0-3416fd1e7056"),
                Amount = -1.23M,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-01"),
                Description = "Transaction 4",
                TransactionType = Models.Enums.TransactionTypeEnum.Card,
                UserCategory = "Bus",
                BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            },

            new Transaction
            {
                Id = new Guid("decf9dcc-bd9e-429d-9414-cb47796fd874"),
                Amount = 167.89M,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-15"),
                Description = "Transaction 5",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "Transfer 1",
                BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
            },

            new Transaction
            {
                Id = new Guid("7691f5f4-be21-4b30-a349-b609ec4aed61"),
                Amount = 890,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-08"),
                Description = "Transaction 6",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "New credits",
                BudgetId = new Guid("593278b6-a28f-48e7-87b8-4ac68e0c05cc"),
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            },

            new Transaction
            {
                Id = new Guid("c79bdcff-7022-4374-8419-f2630a44774b"),
                Amount = -15.56M,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-09"),
                Description = "Transaction 7",
                TransactionType = Models.Enums.TransactionTypeEnum.Card,
                UserCategory = "Restaurant",
                AutoCategory = "Restaurant",
                BudgetId = new Guid("593278b6-a28f-48e7-87b8-4ac68e0c05cc"),
                UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
            },

            new Transaction
            {
                Id = new Guid("182b82d2-96ea-4ea7-9e88-1ba0d0a9f710"),
                Amount = -80,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-02"),
                Description = "Transaction 8",
                TransactionType = Models.Enums.TransactionTypeEnum.Card,
                UserCategory = "Food",
                AutoCategory = "Food",
                BudgetId = new Guid("593278b6-a28f-48e7-87b8-4ac68e0c05cc"),
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            },
        };
    }
}
