using FluentAssertions;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data;
using SplittedUnitTests.Data.FakeRepositoriesData;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.TransactionRepositoryTests
{
    public class TransactionRepositoryDeleteTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public TransactionRepositoryDeleteTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext());
        }


        [Fact]
        public void Test_DeleteExistingTransaction()
        {
            Transaction transaction = FakeTransactionsData.Transactions[1];

            repositoryWrapper.Transactions.Delete(transaction);
            List<Transaction> transactionsFound = repositoryWrapper.Transactions.GetAll();
            Transaction? transactionFound = repositoryWrapper.Transactions
                .GetEntityOrDefaultByCondition(t => t.Id.Equals(transaction.Id));

            transactionsFound.Should().NotBeNull();
            transactionsFound.Should().HaveCount(7);

            transactionFound.Should().BeNull();
        }

        [Fact]
        public void Test_DeleteNonExistingTransaction()
        {
            Transaction transaction = new Transaction
            {
                Id = new Guid("4144d6fa-4935-44aa-8d8a-65d5c7673fe8"),
                Amount = 20M,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-01"),
                Description = "Transaction 11",
                TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                UserCategory = "Transfer 1",
                BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
            };

            repositoryWrapper.Transactions.Delete(transaction);
            List<Transaction> transactionsFound = repositoryWrapper.Transactions.GetAll();

            transactionsFound.Should().NotBeNull();
            transactionsFound.Should().HaveCount(8);
        }

        [Fact]
        public void Test_DeleteExistingTransactions()
        {
            List<Transaction> transactions = new List<Transaction>
            {
                FakeTransactionsData.Transactions[0],
                FakeTransactionsData.Transactions[2],
                FakeTransactionsData.Transactions[4],
                FakeTransactionsData.Transactions[6],
            };

            repositoryWrapper.Transactions.DeleteMultiple(transactions);
            List<Transaction> transactionsFound = repositoryWrapper.Transactions.GetAll();

            transactionsFound.Should().NotBeNull();
            transactionsFound.Should().HaveCount(4);
        }

        [Fact]
        public void Test_DeleteNonExistingTransactions()
        {
            List<Transaction> transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = new Guid("00eb5e0a-b25c-4834-8939-875ba1b60ffa"),
                    Amount = 200M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-02"),
                    Description = "Transaction 12",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    UserCategory = "Credits",
                    BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                    UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
                },

                new Transaction
                {
                    Id = new Guid("34416116-000b-46bd-97c8-622f6a3a88eb"),
                    Amount = -12M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-01"),
                    Description = "Transaction 12",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    UserCategory = "Shopping",
                    BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                    UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
                },

                new Transaction
                {
                    Id = new Guid("23c354aa-4afb-4ca7-a31e-a8fb7eb8cd31"),
                    Amount = -180.45M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-01"),
                    Description = "Transaction 12",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    UserCategory = "Clothes",
                    BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                    UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
                },
            };

            repositoryWrapper.Transactions.DeleteMultiple(transactions);
            List<Transaction> transactionsFound = repositoryWrapper.Transactions.GetAll();

            transactionsFound.Should().NotBeNull();
            transactionsFound.Should().HaveCount(8);
        }

        [Fact]
        public void Test_DeleteExistingAndNonExistingTransactions()
        {
            List<Transaction> transactions = new List<Transaction>
            {
                FakeTransactionsData.Transactions[7],
                new Transaction
                {
                    Id = new Guid("db8c220c-c650-4d6b-a034-f349a9e98513"),
                    Amount = -980M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-06"),
                    Description = "Transaction 13",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    UserCategory = "Furniture",
                    BudgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374"),
                    UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
                },
            };

            repositoryWrapper.Transactions.DeleteMultiple(transactions);
            List<Transaction> transactionsFound = repositoryWrapper.Transactions.GetAll();
            Transaction? transactionFound = repositoryWrapper.Transactions
                .GetEntityOrDefaultByCondition(t => t.Id.Equals(transactions[0].Id));

            transactionsFound.Should().NotBeNull();
            transactionsFound.Should().HaveCount(7);

            transactionFound.Should().BeNull();
        }
    }
}
