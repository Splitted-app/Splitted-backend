using FluentAssertions;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data.FakeRepositoriesData;
using SplittedUnitTests.Data;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.TransactionRepositoryTests
{
    public class TransactionRepositoryCreateTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public TransactionRepositoryCreateTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext(FakeBudgetsData.Budgets,
                FakeTransactionsData.Transactions, FakeGoalsData.Goals));
        }


        [Fact]
        public void Test_CreateSingleTransaction()
        {
            Transaction transaction = new Transaction
            {
                Id = new Guid("993d0058-4f4d-4423-8744-bb9f4abbaf81"),
                Amount = -123,
                Currency = "PLN",
                Date = DateTime.Parse("2023-12-07"),
                Description = "Transaction test",
                TransactionType = Models.Enums.TransactionTypeEnum.Other,
                UserCategory = "Biedronka",
                BudgetId = new Guid("593278b6-a28f-48e7-87b8-4ac68e0c05cc"),
                UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
            };

            repositoryWrapper.Transactions.Create(transaction);
            List<Transaction> transactionsFound = repositoryWrapper.Transactions.GetAll();
            Transaction? transactionFound = repositoryWrapper.Transactions.GetEntityOrDefaultByCondition(t => t.Id.Equals(transaction.Id));

            transactionsFound.Should().NotBeNull();
            transactionsFound.Should().HaveCount(9);

            transactionFound.Should().NotBeNull();
            transactionFound.Should().BeEquivalentTo(transaction);
        }

        [Fact]
        public async Task Test_CreateMultipleTransactionsAsync()
        {
            List<Transaction> transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = new Guid("0ed95d60-5253-4bbf-a6ef-7bd38f1192a1"),
                    Amount = -190,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-04"),
                    Description = "Transaction test 1",
                    TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                    UserCategory = "Food",
                    AutoCategory = "Food",
                    BudgetId = new Guid("593278b6-a28f-48e7-87b8-4ac68e0c05cc"),
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
                },

                new Transaction
                {
                    Id = new Guid("2466ba76-1131-44df-a10f-e8142004cd18"),
                    Amount = -10,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-17"),
                    Description = "Transaction test 2",
                    TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                    UserCategory = "Zabka",
                    BudgetId = new Guid("593278b6-a28f-48e7-87b8-4ac68e0c05cc"),
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
                },

                new Transaction
                {
                    Id = new Guid("83669ac9-81de-464a-b8f8-fc80151bdadb"),
                    Amount = 1000,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-01"),
                    Description = "Transaction test 3",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    UserCategory = "Credits",
                    AutoCategory = "Credits",
                    BudgetId = new Guid("593278b6-a28f-48e7-87b8-4ac68e0c05cc"),
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
                },
            };

            await repositoryWrapper.Transactions.CreateMultipleAsync(transactions);
            List<Transaction> transactionsFound = repositoryWrapper.Transactions.GetAll();

            transactionsFound.Should().NotBeNull();
            transactionsFound.Should().HaveCount(11);
        }

        [Fact]
        public async Task Test_CreateMultipleDuplicatedTransactionsAsync()
        {
            Guid budgetId = new Guid("411ebf58-be4e-4691-ab93-0088bb598374");
            Guid firstDuplicatedTransactionId = new Guid("a7ce392f-923b-491f-9fc0-3416fd1e7056");
            Guid secondDuplicatedTransactionId = new Guid("decf9dcc-bd9e-429d-9414-cb47796fd874");

            List<Transaction> transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = new Guid("a60a066c-bf78-434b-87ae-070fe3a44c15"),
                    Amount = -1.23M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-01"),
                    Description = "Transaction 4",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    UserCategory = "Bus",
                    BudgetId = budgetId,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
                },

                new Transaction
                {
                    Id = new Guid("97ae4759-0113-4ba4-843d-fe261745afc6"),
                    Amount = -1.23M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-01"),
                    Description = "Transaction 4",
                    TransactionType = Models.Enums.TransactionTypeEnum.Card,
                    UserCategory = "Bus",
                    BudgetId = budgetId,
                    UserId = new Guid("f23ad28d-c647-4edd-9bff-dc2c072a066a"),
                },

                new Transaction
                {
                    Id = new Guid("4144d6fa-4935-44aa-8d8a-65d5c7673fe8"),
                    Amount = 167.89M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2023-12-15"),
                    Description = "Transaction 5",
                    TransactionType = Models.Enums.TransactionTypeEnum.Transfer,
                    UserCategory = "Transfer 1",
                    BudgetId = budgetId,
                    UserId = new Guid("bae6d9b0-4a76-493f-b95e-9ceccf3eca11"),
                },
            };

            await repositoryWrapper.Transactions.CreateMultipleAsync(transactions);
            List<Transaction> budgetTransactions = await repositoryWrapper.Transactions
                .GetEntitiesByConditionAsync(t => t.BudgetId.Equals(budgetId));
            repositoryWrapper.Transactions.FindDuplicates(transactions, budgetTransactions);

            budgetTransactions.Should().HaveCount(8);
            transactions[0].DuplicatedTransactionId.Should().Be(firstDuplicatedTransactionId);
            transactions[1].DuplicatedTransactionId.Should().Be(firstDuplicatedTransactionId);
            transactions[2].DuplicatedTransactionId.Should().Be(secondDuplicatedTransactionId);
        }
    }
}
