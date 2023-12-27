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
    public class TransactionRepositoryUpdateTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }


        public TransactionRepositoryUpdateTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext());
        }


        [Fact]
        public void Test_UpdateExistingTransaction()
        {
            DateTime date = DateTime.Parse("2023-11-01");
            decimal amount = 900;

            Transaction transaction = new Transaction
            {
                Id = FakeTransactionsData.Transactions[5].Id,
                Amount = amount,
                Currency = FakeTransactionsData.Transactions[5].Currency,
                Date = date,
                Description = FakeTransactionsData.Transactions[5].Description,
                TransactionType = FakeTransactionsData.Transactions[5].TransactionType,
                UserCategory = FakeTransactionsData.Transactions[5].UserCategory,
                BudgetId = FakeTransactionsData.Transactions[5].BudgetId,
                UserId = FakeTransactionsData.Transactions[5].UserId,
            };

            repositoryWrapper.Transactions.Update(transaction);
            Transaction? transactionFound = repositoryWrapper.Transactions
                .GetEntityOrDefaultByCondition(t => t.Id.Equals(transaction.Id));

            transactionFound.Should().NotBeNull();
            transactionFound.Should().BeEquivalentTo(transaction);
        }

        [Fact]
        public void Test_UpdateNonExistingTransaction()
        {
            DateTime date = DateTime.Parse("2023-11-01");
            decimal amount = 900;

            Transaction transaction = new Transaction
            {
                Id = new Guid("152d63a6-019e-4bbc-a755-bf926147b6bd"),
                Amount = amount,
                Currency = "EUR",
                Date = date,
                Description = "Transaction 10",
                TransactionType = Models.Enums.TransactionTypeEnum.Blik,
                BudgetId = FakeTransactionsData.Transactions[5].BudgetId,
                UserId = FakeTransactionsData.Transactions[5].UserId,
            };

            repositoryWrapper.Transactions.Update(transaction);
            Transaction? transactionFound = repositoryWrapper.Transactions
                .GetEntityOrDefaultByCondition(t => t.Id.Equals(transaction.Id));

            transactionFound.Should().BeNull();
        }
    }
}
