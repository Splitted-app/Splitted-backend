using FluentAssertions;
using Models.DTOs.Incoming.Mode;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Interfaces;
using Splitted_backend.Managers;
using Splitted_backend.Models.Entities;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data.FakeManagersData;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ManagersTests.ModeManagerTests
{
    public class ModeManagerCreateTests
    {
        private IRepositoryWrapper repositoryWrapper { get; }

        private List<User> users { get; }


        public ModeManagerCreateTests()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext(FakeBudgetsData.Budgets,
                FakeTransactionsData.Transactions, new List<Goal>()));
            users = FakeUsersData.Users.ConvertAll(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                Budgets = u.Budgets.ConvertAll(b => (Budget)b.Clone())
            });
        }


        [Fact]
        public async Task Test_CreateFamilyMode()
        {
            FamilyModePostDTO familyModePostDTO = new FamilyModePostDTO
            {
                Bank = BankNameEnum.Ing,
                Name = "FamilyBudget1",
                Currency = "PLN",
            };

            Budget firstPersonalBudget = users[1].Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));
            Budget secondPersonalBudget = users[2].Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));

            Budget expectedBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Family,
                Name = familyModePostDTO.Name,
                Currency = familyModePostDTO.Currency,
                Bank = familyModePostDTO.Bank,
                BudgetBalance = firstPersonalBudget.BudgetBalance + secondPersonalBudget.BudgetBalance,
                CreationDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                Transactions = new List<Transaction>
                {
                    FakeTransactionsData.Transactions[3],
                    FakeTransactionsData.Transactions[4],
                    FakeTransactionsData.Transactions[5],
                    FakeTransactionsData.Transactions[6],
                }
            };

            Budget familyBudget = await ModeManager.CreateFamilyMode(repositoryWrapper, users[1], users[2],
                familyModePostDTO);
            List<Budget> budgets = repositoryWrapper.Budgets.GetAll();

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(11);
            familyBudget.Should().BeEquivalentTo(expectedBudget);

            users[1].Budgets.Should().Contain(familyBudget);
            users[2].Budgets.Should().Contain(familyBudget);
        }

        [Fact]
        public async Task Test_CreateFamilyModeWithDuplicatedTransactions()
        {
            FamilyModePostDTO familyModePostDTO = new FamilyModePostDTO
            {
                Bank = BankNameEnum.Pko,
                Name = "FamilyBudget2",
                Currency = "EUR",
            };

            Budget firstPersonalBudget = users[3].Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));
            Budget secondPersonalBudget = users[4].Budgets.First(b => b.BudgetType.Equals(BudgetTypeEnum.Personal));

            Budget expectedBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Family,
                Name = familyModePostDTO.Name,
                Currency = familyModePostDTO.Currency,
                Bank = familyModePostDTO.Bank,
                BudgetBalance = firstPersonalBudget.BudgetBalance + secondPersonalBudget.BudgetBalance,
                CreationDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                Transactions = new List<Transaction>
                {
                    FakeTransactionsData.Transactions[7],
                    FakeTransactionsData.Transactions[8],
                    FakeTransactionsData.Transactions[9],
                    FakeTransactionsData.Transactions[10],
                }
            };

            Budget familyBudget = await ModeManager.CreateFamilyMode(repositoryWrapper, users[4], users[3],
                familyModePostDTO);
            List<Budget> budgets = repositoryWrapper.Budgets.GetAll();

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(11);

            familyBudget.Should().BeEquivalentTo(expectedBudget);
            familyBudget.Transactions[2].DuplicatedTransactionId.Should()
                .Be(new Guid("bf6d3827-e7bc-4531-8c77-0337eb63c52a"));
            familyBudget.Transactions[3].DuplicatedTransactionId.Should()
                .Be(new Guid("bf6d3827-e7bc-4531-8c77-0337eb63c52a"));

            users[3].Budgets.Should().Contain(familyBudget);
            users[4].Budgets.Should().Contain(familyBudget);
        }

        [Fact]
        public async Task Test_CreatePartnerMode()
        {
            PartnerModePostDTO partnerModePostDTO = new PartnerModePostDTO
            {
                Name = "PartnerBudget",
            };

            Budget expectedBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Partner,
                Name = partnerModePostDTO.Name,
                Currency = string.Empty,
                CreationDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
            };

            Budget partnerBudget = await ModeManager.CreatePartnerMode(repositoryWrapper, users[4], users[5],
                partnerModePostDTO);
            List<Budget> budgets = repositoryWrapper.Budgets.GetAll();

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(13);
            partnerBudget.Should().BeEquivalentTo(expectedBudget);

            users[4].Budgets.Should().HaveCount(2);
            users[4].Budgets.Should().Contain(partnerBudget);
            users[5].Budgets.Should().HaveCount(2);
            users[5].Budgets.Should().Contain(partnerBudget);
        }
        
        [Fact]
        public async Task Test_CreateTemporaryMode()
        {
            TemporaryModePostDTO temporaryModePostDTO = new TemporaryModePostDTO
            {
                Name = "TemporaryBudget",
            };

            List<User> otherUsers = new List<User>
            {
                users[5],
                users[6],
                users[7]
            };

            Budget expectedBudget = new Budget
            {
                BudgetType = BudgetTypeEnum.Temporary,
                Name = temporaryModePostDTO.Name,
                Currency = string.Empty,
                CreationDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
            };

            Budget temporaryBudget = await ModeManager.CreateTemporaryMode(repositoryWrapper, users[4], otherUsers, 
                temporaryModePostDTO);
            List<Budget> budgets = repositoryWrapper.Budgets.GetAll();

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(13);
            temporaryBudget.Should().BeEquivalentTo(expectedBudget);

            users[4].Budgets.Should().HaveCount(2);
            users[4].Budgets.Should().Contain(temporaryBudget);
            users[5].Budgets.Should().HaveCount(2);
            users[5].Budgets.Should().Contain(temporaryBudget);
            users[6].Budgets.Should().HaveCount(2);
            users[6].Budgets.Should().Contain(temporaryBudget);
            users[7].Budgets.Should().HaveCount(2);
            users[7].Budgets.Should().Contain(temporaryBudget);
        }
    }
}
