using Microsoft.AspNetCore.Identity;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Incoming.Mode;
using Models.DTOs.Outgoing.Budget;
using Models.Entities;
using Models.Enums;
using Org.BouncyCastle.Security.Certificates;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;
using Splitted_backend.Repositories;
using System.Runtime.CompilerServices;

namespace Splitted_backend.Managers
{
    public static class ModeManager
    {
        public static async Task<Budget> CreateFamilyMode(IRepositoryWrapper repositoryWrapper, User firstUser,
            User secondUser, FamilyModePostDTO familyModePostDTO)
        {
            Budget firstPersonalBudget = firstUser.Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));
            Budget secondPersonalBudget = secondUser.Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));

            Budget familyBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Family,
                Name = familyModePostDTO.Name,
                Currency = familyModePostDTO.Currency,
                Bank = familyModePostDTO.Bank,
                BudgetBalance = firstPersonalBudget.BudgetBalance + secondPersonalBudget.BudgetBalance,
                CreationDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
            };

            repositoryWrapper.Transactions.FindDuplicates(secondPersonalBudget.Transactions, firstPersonalBudget.Transactions);
            familyBudget.Transactions.AddRange(firstPersonalBudget.Transactions);
            familyBudget.Transactions.AddRange(secondPersonalBudget.Transactions);

            repositoryWrapper.Budgets.Create(familyBudget);
            firstUser.Budgets.Add(familyBudget);
            secondUser.Budgets.Add(familyBudget);

            repositoryWrapper.Budgets.Delete(firstPersonalBudget);
            repositoryWrapper.Budgets.Delete(secondPersonalBudget);

            await repositoryWrapper.SaveChanges();
            return familyBudget;
        }

        public static async Task<Budget> CreatePartnerMode(IRepositoryWrapper repositoryWrapper, User firstUser, 
            User secondUser, PartnerModePostDTO partnerModePostDTO)
        {
            Budget partnerBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Partner,
                Name = partnerModePostDTO.Name,
                Currency = string.Empty,
                CreationDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
            };

            repositoryWrapper.Budgets.Create(partnerBudget);
            firstUser.Budgets.Add(partnerBudget);
            secondUser.Budgets.Add(partnerBudget);

            await repositoryWrapper.SaveChanges();
            return partnerBudget;
        }

        public static async Task<Budget> CreateTemporaryMode(IRepositoryWrapper repositoryWrapper, User firstUser,
            List<User> otherUsers, TemporaryModePostDTO temporaryModePostDTO)
        {
            Budget temporaryBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Temporary,
                Name = temporaryModePostDTO.Name,
                Currency = string.Empty,
                CreationDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
            };

            repositoryWrapper.Budgets.Create(temporaryBudget);
            firstUser.Budgets.Add(temporaryBudget);
            otherUsers.ForEach(ou => ou.Budgets.Add(temporaryBudget));

            await repositoryWrapper.SaveChanges();
            return temporaryBudget;
        }

        public static void LeaveMode(IRepositoryWrapper repositoryWrapper,
            User? user, List<User> otherUsers, Budget budget)
        {
            if (budget.BudgetType.Equals(BudgetTypeEnum.Family))
            {
                User otherUser = otherUsers[0];

                CreatePersonalBudgetFromFamilyMode(repositoryWrapper, user, budget);
                CreatePersonalBudgetFromFamilyMode(repositoryWrapper, otherUser, budget);
            }
            else
            {
                if (user is not null)
                {
                    user.Budgets.Remove(budget);
                    budget.Transactions.RemoveAll(t => t.UserId.Equals(user.Id));

                    budget.Transactions.ForEach(t => t.TransactionPayBacks.RemoveAll(tpb =>
                        tpb.OwingUserId.Equals(user.Id) || tpb.OwedUserId.Equals(user.Id)));
                }
            }
        }

        private static void CreatePersonalBudgetFromFamilyMode(IRepositoryWrapper repositoryWrapper, User? user,
            Budget familyBudget)
        {
            if (user is null)
                return;

            Budget budget = new Budget
            {
                BudgetType = BudgetTypeEnum.Personal,
                Currency = familyBudget.Currency,
                Name = string.Empty,
                Bank = familyBudget.Bank,
                BudgetBalance = 0,
                CreationDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
            };

            repositoryWrapper.Budgets.Create(budget);

            familyBudget.Transactions.ForEach(t =>
            {
                if (t.UserId.Equals(user.Id)) t.BudgetId = budget.Id;
            });

            user.Budgets.Add(budget);
        }

        public static void DeterminePayBacks(List<Transaction> transactions, Guid userId, List<Guid> otherUserIds)
        {
            foreach (Transaction transaction in transactions)
            {
                List<TransactionPayBack> transactionPayBacks = new List<TransactionPayBack>();

                if (transaction.Amount > 0)
                { 
                    otherUserIds.ForEach(ui => transactionPayBacks.Add(new TransactionPayBack
                    {
                        Amount = -transaction.Amount / (otherUserIds.Count + 1),
                        TransactionPayBackStatus = TransactionPayBackStatusEnum.Unpaid,
                        OwingUserId = userId,
                        OwedUserId = ui,
                    }));
                }
                else
                {
                    otherUserIds.ForEach(ui => transactionPayBacks.Add(new TransactionPayBack
                    {
                        Amount = transaction.Amount / (otherUserIds.Count + 1),
                        TransactionPayBackStatus = TransactionPayBackStatusEnum.Unpaid,
                        OwingUserId = ui,
                        OwedUserId = userId,
                    }));
                }

                transaction.TransactionPayBacks.AddRange(transactionPayBacks);
            }
        }

        public static void MakePayback(Transaction transaction, Guid? paybackTransactionId, Guid userId)
        {
            List<TransactionPayBack> transactionPayBacks = transaction.TransactionPayBacks
                .Where(tpb => tpb.OwingUserId.Equals(userId))
                .ToList();

            transactionPayBacks.ForEach(tpb =>
            {
                tpb.TransactionPayBackStatus = TransactionPayBackStatusEnum.WaitingForApproval;

                if (paybackTransactionId is null)
                    tpb.InCash = true;
                else
                    tpb.PayBackTransactionId = paybackTransactionId;
            });
        }

        public static void ResolvePayback(Transaction transaction, Guid transactionPayBackId, bool accept)
        {
            TransactionPayBack transactionPayBack = transaction.TransactionPayBacks
                .First(tpb => tpb.Id.Equals(transactionPayBackId));

            if (accept)
                transactionPayBack.TransactionPayBackStatus = TransactionPayBackStatusEnum.PaidBack;
            else
            {
                transactionPayBack.TransactionPayBackStatus = TransactionPayBackStatusEnum.Unpaid;
                transactionPayBack.PayBackTransactionId = null;
                transactionPayBack.InCash = false;
            }            
        }

        public static decimal GetUserDebt(Budget budget, Guid userId)
        {
            decimal debt = budget.Transactions.Aggregate(0M, (current, next) =>
            {
                return current + next.TransactionPayBacks.Aggregate(0M, (currentDebt, nextPayBack) =>
                {
                    if (nextPayBack.OwingUserId.Equals(userId) && 
                    !nextPayBack.TransactionPayBackStatus.Equals(TransactionPayBackStatusEnum.PaidBack))
                        return currentDebt + nextPayBack.Amount;
                    else
                        return currentDebt;
                });
            });

            decimal income = budget.Transactions.Aggregate(0M, (current, next) =>
            {
                return current + next.TransactionPayBacks.Aggregate(0M, (currentIncome, nextPayBack) =>
                {
                    if (nextPayBack.OwedUserId.Equals(userId) &&
                    !nextPayBack.TransactionPayBackStatus.Equals(TransactionPayBackStatusEnum.PaidBack))
                        return currentIncome + Math.Abs(nextPayBack.Amount);
                    else
                        return currentIncome;
                });
            });

            return debt + income;
        }
    }
}
