using Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Data.FakeManagersData
{
    public static class FakeTransactionsData
    {
        public static List<Transaction> Transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = new Guid("a0639cdf-78d4-4910-a5a6-10e7616bf8d2"),
                Amount = -900,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-19"),
                Description = "Transaction 22",
                TransactionType = Models.Enums.TransactionTypeEnum.Card,
                UserCategory = "Clothes",
                BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
            },

            new Transaction
            {
                Id = new Guid("a0639cdf-78d4-4910-a5a6-10e7616bf8d2"),
                Amount = -1000,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-19"),
                Description = "Transaction 21",
                TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                UserCategory = "Clothes",
                BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                UserId = new Guid("169115ae-0921-4f8d-ad3b-cd3b72c1a943"),
            },

            new Transaction
            {
                Id = new Guid("a0639cdf-78d4-4910-a5a6-10e7616bf8d2"),
                Amount = -1200,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-21"),
                Description = "Transaction 20",
                TransactionType = Models.Enums.TransactionTypeEnum.Card,
                UserCategory = "Clothes",
                BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                UserId = new Guid("169115ae-0921-4f8d-ad3b-cd3b72c1a943"),
            },

            new Transaction
            {
                Id = new Guid("336f3cd1-c843-4997-a772-4c2a69e9b8f7"),
                Amount = -150,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-02"),
                Description = "Transaction 1",
                TransactionType = Models.Enums.TransactionTypeEnum.Card,
                UserCategory = "Shopping",
                BudgetId = new Guid("98cd1dee-274c-405e-aa82-b2147ab5bc13"),
                UserId = new Guid("7645b7b5-2d7b-4122-8e16-cef9df13bb4c"),
            },

            new Transaction
            {
                Id = new Guid("9f7ffcea-2005-4e19-a323-cb6b6f46aeaf"),
                Amount = 234.45M,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-12"),
                Description = "Transaction 2",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "Clothes",
                BudgetId = new Guid("98cd1dee-274c-405e-aa82-b2147ab5bc13"),
                UserId = new Guid("7645b7b5-2d7b-4122-8e16-cef9df13bb4c"),
            },

            new Transaction
            {
                Id = new Guid("d4884acf-6d97-48b0-a7c6-39678cf2fab2"),
                Amount = -1902,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-11"),
                Description = "Transaction 3",
                TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                UserCategory = "Shopping",
                BudgetId = new Guid("98cd1dee-274c-405e-aa82-b2147ab5bc13"),
                UserId = new Guid("7645b7b5-2d7b-4122-8e16-cef9df13bb4c"),
            },

            new Transaction
            {
                Id = new Guid("8ea1e92f-0928-44f9-9c62-aa8800f76714"),
                Amount = -1.23M,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-01"),
                Description = "Transaction 4",
                TransactionType = Models.Enums.TransactionTypeEnum.Card,
                UserCategory = "Bus",
                BudgetId = new Guid("9ffafd1b-4b2b-426d-8dd9-e2634df28c75"),
                UserId = new Guid("e0cbb7d6-556d-43e7-82da-6e2b9938e359"),
            },
            
            new Transaction
            {
                Id = new Guid("4ce1c223-7792-4873-b0df-15ddb9b26113"),
                Amount = 167.89M,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-15"),
                Description = "Transaction 5",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "Transfer 1",
                BudgetId = new Guid("06365522-6b68-4da3-b8e5-11164d27551e"),
                UserId = new Guid("37320745-d376-4bde-9346-07870ea926a5"),
            },

            new Transaction
            {
                Id = new Guid("14157ba5-1bb5-436b-9ff4-e21e9be37a38"),
                Amount = 890,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-08"),
                Description = "Transaction 6",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "New credits",
                BudgetId = new Guid("06365522-6b68-4da3-b8e5-11164d27551e"),
                UserId = new Guid("37320745-d376-4bde-9346-07870ea926a5"),
            },

            new Transaction
            {
                Id = new Guid("d2297c7a-9b8c-48cd-8d1a-f22d80b550f2"),
                Amount = 890,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-08"),
                Description = "Transaction 6",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "New credits",
                BudgetId = new Guid("06365522-6b68-4da3-b8e5-11164d27551e"),
                UserId = new Guid("37320745-d376-4bde-9346-07870ea926a5"),
            },

            new Transaction
            {
                Id = new Guid("bf6d3827-e7bc-4531-8c77-0337eb63c52a"),
                Amount = 890,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-08"),
                Description = "Transaction 6",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "New credits",
                BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                UserId = new Guid("60942901-7fed-4bf6-8871-4946e95e6695"),
            },

            new Transaction
            {
                Id = new Guid("a0639cdf-78d4-4910-a5a6-10e7616bf8d2"),
                Amount = 900,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-19"),
                Description = "Transaction 16",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "New credits 1",
                BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                UserId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
            },

            new Transaction
            {
                Id = new Guid("a0639cdf-78d4-4910-a5a6-10e7616bf8d2"),
                Amount = 1000,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-19"),
                Description = "Transaction 17",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "New credits 2",
                BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                UserId = new Guid("169115ae-0921-4f8d-ad3b-cd3b72c1a943"),
            },

            new Transaction
            {
                Id = new Guid("a0639cdf-78d4-4910-a5a6-10e7616bf8d2"),
                Amount = 1200,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-21"),
                Description = "Transaction 18",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "New credits 3",
                BudgetId = new Guid("87ed2d85-d00d-4fef-a7ff-1049f96420f9"),
                UserId = new Guid("169115ae-0921-4f8d-ad3b-cd3b72c1a943"),
            },
        };
    }
}
