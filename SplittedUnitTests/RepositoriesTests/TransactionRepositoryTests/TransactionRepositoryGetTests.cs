using FluentAssertions;
using Models.Entities;
using Models.Enums;
using SplittedUnitTests.RepositoriesTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.TransactionRepositoryTests
{
    public class TransactionRepositoryGetTests : IClassFixture<RepositoryGetTestFixture>
    {
        private RepositoryGetTestFixture repositoryGetTestFixture { get; }


        public TransactionRepositoryGetTests(RepositoryGetTestFixture repositoryGetTestFixture)
        {
            this.repositoryGetTestFixture = repositoryGetTestFixture;
        }


        [Fact]
        public void Test_GetExistingTransactionById()
        {
            Guid id = new Guid("a7ce392f-923b-491f-9fc0-3416fd1e7056");

            Transaction? transaction = repositoryGetTestFixture.repositoryWrapper.Transactions
                .GetEntityOrDefaultByCondition(t => t.Id.Equals(id));

            transaction.Should().NotBeNull();
            transaction.Should().BeEquivalentTo(new Transaction
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
            });
        }

        [Fact]
        public void Test_GetNonExistingTransactionById()
        {
            Guid id = new Guid("bda3eec3-fc62-4bc5-aaea-1dd8cf4c1bc4");

            Transaction? transaction = repositoryGetTestFixture.repositoryWrapper.Transactions
                .GetEntityOrDefaultByCondition(t => t.Id.Equals(id));
            
            transaction.Should().BeNull();
        }

        [Fact]
        public async Task Test_GetExistingTransactionBydIdAsync()
        {
            Guid id = new Guid("3dba0f55-f0a4-4b92-9c71-ac0408fcfca4");

            Transaction? transaction = await repositoryGetTestFixture.repositoryWrapper.Transactions
                .GetEntityOrDefaultByConditionAsync(t => t.Id.Equals(id));

            transaction.Should().NotBeNull();
            transaction.Should().BeEquivalentTo(new Transaction
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
            });
        }

        [Fact]
        public async Task Test_GetNonExistingTransactionBydIdAsync()
        {
            Guid id = new Guid("652d0b48-e93f-4138-94ac-d9add3d6a2c5");

            Transaction? transaction = await repositoryGetTestFixture.repositoryWrapper.Transactions
                .GetEntityOrDefaultByConditionAsync(t => t.Id.Equals(id));

            transaction.Should().BeNull();
        }

        [Fact]
        public async Task Test_GetAllTransactionsAsync()
        {
            List<Transaction> transactions = await repositoryGetTestFixture.repositoryWrapper.Transactions.GetAllAsync();

            transactions.Should().NotBeNull();
            transactions.Should().HaveCount(8);
        }

        [Fact]
        public async Task Test_GetExistingTransactionsByTransactionTypeAsync()
        {
            TransactionTypeEnum transactionType = TransactionTypeEnum.Card;
            List<Transaction> expectedTransactions = new List<Transaction>
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

            List<Transaction> transactions = await repositoryGetTestFixture.repositoryWrapper.Transactions
                .GetEntitiesByConditionAsync(t => t.TransactionType.Equals(transactionType));

            transactions.Should().NotBeNull();
            transactions.Should().HaveCount(4);
            transactions.Should().BeEquivalentTo(expectedTransactions);
        }

        [Fact]
        public async Task Test_GetNonExistingTransactionsByTransactionTypeAsync()
        {
            TransactionTypeEnum transactionType = TransactionTypeEnum.Other;

            List<Transaction> transactions = await repositoryGetTestFixture.repositoryWrapper.Transactions
                .GetEntitiesByConditionAsync(t => t.TransactionType.Equals(transactionType));

            transactions.Should().NotBeNull();
            transactions.Should().HaveCount(0);
        }
    }
}
