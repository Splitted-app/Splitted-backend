using FluentAssertions;
using Models.Entities;
using Org.BouncyCastle.Asn1.Sec;
using Splitted_backend.Interfaces;
using Splitted_backend.Managers;
using Splitted_backend.Models.Entities;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data.FakeManagersData;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ManagersTests.ModeManagerTests
{
    public class ModeManagerPaybacksTests
    {
        private List<Transaction> transactions { get; }


        public ModeManagerPaybacksTests()
        {
            transactions = FakeTransactionsData.Transactions.ConvertAll(t => (Transaction)t.Clone());
        }


        [Theory]
        [InlineData(1, "c6b2e9c7-fa88-43c5-887d-bf79eeab8cda", 3)]
        [InlineData(2, "169115ae-0921-4f8d-ad3b-cd3b72c1a943", 2)]
        [InlineData(3, "169115ae-0921-4f8d-ad3b-cd3b72c1a943", 1)]
        public void Test_DeterminePayBacksWhenIncome(int transactionsCount, Guid userId, int otherUserCount)
        {
            List<Transaction> transactionsTaken = transactions.TakeLast(transactionsCount)
                .ToList();

            List<Guid> otherUserIds = FakeUsersData.Users.TakeLast(otherUserCount)
                .Select(u => u.Id)
                .ToList();

            List<List<TransactionPayBack>> expectedPayBacks = new List<List<TransactionPayBack>>();
            int i = 0;
            transactionsTaken.ForEach(t =>
            {
                expectedPayBacks.Add(new List<TransactionPayBack>());

                otherUserIds.ForEach(oui => expectedPayBacks[i].Add(new TransactionPayBack
                {
                    Amount = -t.Amount / (otherUserCount + 1),
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.Unpaid,
                    OwingUserId = userId,
                    OwedUserId = oui
                }));
                i++;
            });

            ModeManager.DeterminePayBacks(transactionsTaken, userId, otherUserIds);

            i = 0;
            transactionsTaken.ForEach(t =>
            {
                t.TransactionPayBacks.Should().BeEquivalentTo(expectedPayBacks[i++]);
            });
        }

        [Theory]
        [InlineData(1, "c6b2e9c7-fa88-43c5-887d-bf79eeab8cda", 3)]
        [InlineData(2, "169115ae-0921-4f8d-ad3b-cd3b72c1a943", 2)]
        [InlineData(3, "169115ae-0921-4f8d-ad3b-cd3b72c1a943", 1)]
        public void Test_DeterminePayBacksWhenExpenses(int transactionsCount, Guid userId, int otherUserCount)
        {
            List<Transaction> transactionsTaken = transactions.Take(transactionsCount)
                .ToList();

            List<Guid> otherUserIds = FakeUsersData.Users.TakeLast(otherUserCount)
                .Select(u => u.Id)
                .ToList();

            List<List<TransactionPayBack>> expectedPayBacks = new List<List<TransactionPayBack>>();
            int i = 0;
            transactionsTaken.ForEach(t =>
            {
                expectedPayBacks.Add(new List<TransactionPayBack>());

                otherUserIds.ForEach(oui => expectedPayBacks[i].Add(new TransactionPayBack
                {
                    Amount = t.Amount / (otherUserCount + 1),
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.Unpaid,
                    OwingUserId = oui,
                    OwedUserId = userId
                }));
                i++;
            });

            ModeManager.DeterminePayBacks(transactionsTaken, userId, otherUserIds);

            i = 0;
            transactionsTaken.ForEach(t =>
            {
                t.TransactionPayBacks.Should().BeEquivalentTo(expectedPayBacks[i++]);
            });
        }

        [Theory]
        [InlineData("a0639cdf-78d4-4910-a5a6-10e7616bf8d2", false)]
        [InlineData(null, true)]
        public void Test_MakePayback(string? paybackTransactionIdString, bool inCash)
        {
            Guid? paybackTransactionId = paybackTransactionIdString is null ? null 
                : new Guid(paybackTransactionIdString);
            Guid userId1 = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda");
            Guid userId2 = new Guid("3da39e43-2299-445a-a9d1-83bbc0671afa");

            Transaction transaction = transactions[transactions.Count - 2];
            transaction.TransactionPayBacks = new List<TransactionPayBack>
            {
                new TransactionPayBack
                {
                    Id = new Guid("da5f3200-a323-49d2-8f33-178f8becf27d"),
                    Amount = transaction.Amount / 3,
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.Unpaid,
                    OwingUserId = transaction.UserId,
                    OwedUserId = userId1
                },

                new TransactionPayBack
                {
                    Id = new Guid("1fd89d25-9209-4d13-814c-bbc138812636"),
                    Amount = transaction.Amount / 3,
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.Unpaid,
                    OwingUserId = transaction.UserId,
                    OwedUserId = userId2
                },
            };

            ModeManager.MakePayback(transaction, paybackTransactionId, transaction.UserId);

            transaction.TransactionPayBacks.ForEach(tpb =>
            {
                tpb.TransactionPayBackStatus.Should().Be(Models.Enums.TransactionPayBackStatusEnum.WaitingForApproval);
                tpb.InCash.Should().Be(inCash);
                tpb.PayBackTransactionId.Should().Be(paybackTransactionId);
            });
        }

        [Theory]
        [InlineData("a0639cdf-78d4-4910-a5a6-10e7616bf8d2", false)]
        [InlineData(null, true)]
        public void Test_ResolvePaybackAndReject(string? paybackTransactionIdString, bool inCash)
        {
            Guid? paybackTransactionId = paybackTransactionIdString is null ? null
                : new Guid(paybackTransactionIdString);
            Guid userId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda");
            Transaction transaction = transactions[1];

            transaction.TransactionPayBacks = new List<TransactionPayBack>
            {
                new TransactionPayBack
                {
                    Id = new Guid("6236fc27-4af4-4d30-8332-b5650cb87580"),
                    Amount = transaction.Amount / 2,
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.WaitingForApproval,
                    OwingUserId = userId,
                    OwedUserId = transaction.UserId,
                    InCash = inCash,
                    PayBackTransactionId = paybackTransactionId,
                },
            };

            ModeManager.ResolvePayback(transaction, transaction.TransactionPayBacks[0].Id, false);

            transaction.TransactionPayBacks[0].TransactionPayBackStatus.Should().Be(
                Models.Enums.TransactionPayBackStatusEnum.Unpaid);
            transaction.TransactionPayBacks[0].InCash.Should().Be(false);
            transaction.TransactionPayBacks[0].PayBackTransactionId.Should().Be(null);
        }

        [Theory]
        [InlineData("a0639cdf-78d4-4910-a5a6-10e7616bf8d2", false)]
        [InlineData(null, true)]
        public void Test_ResolvePaybackAndAccept(string? paybackTransactionIdString, bool inCash)
        {
            Guid? paybackTransactionId = paybackTransactionIdString is null ? null
                : new Guid(paybackTransactionIdString);
            Guid userId = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda");
            Transaction transaction = transactions[1];

            transaction.TransactionPayBacks = new List<TransactionPayBack>
            {
                new TransactionPayBack
                {
                    Id = new Guid("6236fc27-4af4-4d30-8332-b5650cb87580"),
                    Amount = transaction.Amount / 2,
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.WaitingForApproval,
                    OwingUserId = userId,
                    OwedUserId = transaction.UserId,
                    InCash = inCash,
                    PayBackTransactionId = paybackTransactionId,
                },
            };

            ModeManager.ResolvePayback(transaction, transaction.TransactionPayBacks[0].Id, true);

            transaction.TransactionPayBacks[0].TransactionPayBackStatus.Should().Be(
                Models.Enums.TransactionPayBackStatusEnum.PaidBack);
            transaction.TransactionPayBacks[0].InCash.Should().Be(inCash);
            transaction.TransactionPayBacks[0].PayBackTransactionId.Should().Be(paybackTransactionId);
        }

        [Fact]
        public void Test_GetUserDebt()
        {
            Guid firstUserId = new Guid("e449ef7c-665e-4502-8347-4b33a1b715a9");
            Guid secondUserId = new Guid("169115ae-0921-4f8d-ad3b-cd3b72c1a943");

            Budget budget = FakeBudgetsData.Budgets[FakeBudgetsData.Budgets.Count - 1];
            budget.Transactions = new List<Transaction>
            {
                (Transaction)FakeTransactionsData.Transactions[0].Clone(),
                (Transaction)FakeTransactionsData.Transactions[3].Clone(),
                (Transaction)FakeTransactionsData.Transactions[8].Clone(),
                (Transaction)FakeTransactionsData.Transactions[12].Clone(),
            };

            budget.Transactions[0].TransactionPayBacks = new List<TransactionPayBack>
            {
                new TransactionPayBack
                {
                    Id = new Guid("762de575-a108-4b82-8888-ac70e47adc22"),
                    Amount = budget.Transactions[0].Amount / 2,
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.WaitingForApproval,
                    OwingUserId = firstUserId,
                    OwedUserId = secondUserId,
                    InCash = true,
                }
            };

            budget.Transactions[1].TransactionPayBacks = new List<TransactionPayBack>
            {
                new TransactionPayBack
                {
                    Id = new Guid("6446fffb-b6dc-48c9-8661-359b59973493"),
                    Amount = budget.Transactions[1].Amount / 2,
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.Unpaid,
                    OwingUserId = firstUserId,
                    OwedUserId = secondUserId,
                }
            };

            budget.Transactions[2].TransactionPayBacks = new List<TransactionPayBack>
            {
                new TransactionPayBack
                {
                    Id = new Guid("2b8ae958-71f7-4053-a236-56d9e12d13cf"),
                    Amount = -budget.Transactions[2].Amount / 2,
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.PaidBack,
                    OwingUserId = secondUserId,
                    OwedUserId = firstUserId,
                    InCash = true,
                }
            };

            budget.Transactions[3].TransactionPayBacks = new List<TransactionPayBack>
            {
                new TransactionPayBack
                {
                    Id = new Guid("626d7dd5-dbd2-4596-b4ae-dacc329e3629"),
                    Amount = -budget.Transactions[3].Amount / 2,
                    TransactionPayBackStatus = Models.Enums.TransactionPayBackStatusEnum.Unpaid,
                    OwingUserId = secondUserId,
                    OwedUserId = firstUserId,
                }
            };

            decimal firstUserDebt = ModeManager.GetUserDebt(budget, firstUserId);
            decimal secondUserDebt = ModeManager.GetUserDebt(budget, secondUserId);

            firstUserDebt.Should().Be(-25);
            secondUserDebt.Should().Be(25);
        }
    }
}
