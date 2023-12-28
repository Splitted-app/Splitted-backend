using FluentAssertions;
using Models.DTOs.Incoming.Mode;
using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Managers;
using Splitted_backend.Models.Entities;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data.FakeManagersData;
using SplittedUnitTests.Data.FakeRepositoriesData;
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
                FakeTransactionsData.Transactions, FakeGoalsData.Goals));
            users = FakeUsersData.Users.ConvertAll(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                Budgets = u.Budgets.ConvertAll(b => (Budget)b.Clone())
            });
        }

        [Fact]
        public async Task Test_CreatePartnerMode()
        {
            PartnerModePostDTO partnerModePostDTO = new PartnerModePostDTO
            {
                Name = "PartnerBudget",
            };

            Budget partnerBudget = await ModeManager.CreatePartnerMode(repositoryWrapper, users[4], users[5],
                partnerModePostDTO);
            List<Budget> budgets = repositoryWrapper.Budgets.GetAll();

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(11);
            partnerBudget.Name.Should().Be("PartnerBudget");

            users[4].Budgets.Should().HaveCount(2);
            users[4].Budgets.Should().Contain(partnerBudget);
            users[5].Budgets.Should().HaveCount(2);
            users[5].Budgets.Should().Contain(partnerBudget);
        }
        
        [Fact]
        public async Task Test_CreateFamilyMode()
        {
            PartnerModePostDTO partnerModePostDTO = new PartnerModePostDTO
            {
                Name = "PartnerBudget",
            };

            Budget partnerBudget = await ModeManager.CreatePartnerMode(repositoryWrapper, users[4], users[5], 
                partnerModePostDTO);
            List<Budget> budgets = repositoryWrapper.Budgets.GetAll();

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(11);
            partnerBudget.Name.Should().Be("PartnerBudget");

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

            Budget temporaryBudget = await ModeManager.CreateTemporaryMode(repositoryWrapper, users[4], otherUsers, 
                temporaryModePostDTO);
            List<Budget> budgets = repositoryWrapper.Budgets.GetAll();

            budgets.Should().NotBeNull();
            budgets.Should().HaveCount(11);
            temporaryBudget.Name.Should().Be("TemporaryBudget");

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
