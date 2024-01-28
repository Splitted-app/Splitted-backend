using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Models.DTOs.Outgoing.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.Net;
using SplittedIntegrationTests.CustomWebApp;
using Respawn;
using Models.DTOs.Incoming.User;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Splitted_backend.Interfaces;
using Microsoft.AspNetCore.Identity;
using Splitted_backend.Models.Entities;
using Azure;
using System.Net.Http.Headers;
using SplittedIntegrationTests.Mocks;
using Org.BouncyCastle.Math.EC.Endo;
using Models.DTOs.Outgoing.Budget;
using Models.Enums;
using Models.DTOs.Outgoing.Goal;

namespace SplittedIntegrationTests.ControllersTests.UserControllerTests
{
    [Collection("WebApp Collection")]
    public class UserControllerGetTests : IAsyncLifetime
    {
        public DatabaseFixture databaseFixture { get; }

        public CustomWebApplicationFactory factory { get; }

        public HttpClient httpClient { get; }

        private string token { get; set; } = null!;


        public UserControllerGetTests(CustomWebApplicationFactory factory, DatabaseFixture databaseFixture) 
        {
            this.databaseFixture = databaseFixture;
            this.factory = factory;
            this.httpClient = factory.CreateClient();
        }


        [Theory]
        [InlineData(123, "Email is invalid.")]
        [InlineData("baddata", "Email is invalid.")]
        public async Task Test_CheckUserEmailWithInvalidData(object email, string expectedMessage)
        {
            string checkEmailUri = $"/api/users/email-check?email={email}";

            HttpResponseMessage response = await httpClient.GetAsync(checkEmailUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            string responseMessage = await response.Content.ReadAsStringAsync();
            responseMessage.Should().Be(expectedMessage);
        }

        [Theory]
        [InlineData("mateusz@example.com", true)]
        [InlineData("test@example.com", false)]
        public async Task Test_CheckUserEmailWithProperData(string email, bool exists) 
        {
            string checkEmailUri = $"/api/users/email-check?email={email}";

            HttpResponseMessage response = await httpClient.GetAsync(checkEmailUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            UserEmailCheckDTO userEmailCheckDTO = JsonConvert.DeserializeObject<UserEmailCheckDTO>(
                await response.Content.ReadAsStringAsync());

            userEmailCheckDTO.Should().NotBeNull();
            userEmailCheckDTO.Should().BeEquivalentTo(new UserEmailCheckDTO
            {
                UserExists = exists
            });
        }

        [Fact]
        public async Task Test_GetUser()
        {
            string getUserUri = "/api/users";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.GetAsync(getUserUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            UserGetDTO userEmailCheckDTO = JsonConvert.DeserializeObject<UserGetDTO>(
                await response.Content.ReadAsStringAsync());

            userEmailCheckDTO.Should().NotBeNull();
            userEmailCheckDTO.Should().BeEquivalentTo(new UserGetDTO
            {
                Email = "mateusz@example.com",
                Username = "mati",
                AvatarImage = null,
                EmailConfirmed = false,
            }, options => options.Excluding(u => u.Id));
        }

        [Theory]
        [InlineData("wrongType")]
        [InlineData("personal, wrongType")]
        public async Task Test_GetUserBudgetsWithInvalidData(string? budgetType)
        {
            string getUserBudgetsUri = $"/api/users/budgets?budgetType={budgetType}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.GetAsync(getUserBudgetsUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData(null, 2)]
        [InlineData("personal", 1)]
        [InlineData("temporary", 0)]
        [InlineData("personal, temporary", 1)]
        public async Task Test_GetUserBudgetsWithProperData(string? budgetType, int count)
        {
            string getUserBudgetsUri = $"/api/users/budgets?budgetType={budgetType}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.GetAsync(getUserBudgetsUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<UserBudgetGetDTO> userBudgetGetDTOs = JsonConvert.DeserializeObject<List<UserBudgetGetDTO>>(
                await response.Content.ReadAsStringAsync());

            userBudgetGetDTOs.Should().NotBeNull();
            userBudgetGetDTOs.Should().HaveCount(count);

            if (count == 0) return;

            userBudgetGetDTOs[0].Should().BeEquivalentTo(new UserBudgetGetDTO
            {
                Bank = BankNameEnum.Ing,
                BudgetType = BudgetTypeEnum.Personal,
                Name = string.Empty,
                Currency = "PLN",
                BudgetBalance = 10000,
            }, options => options.Excluding(u => u.Id));
        }

        [Fact]
        public async Task Test_GetUserFriends()
        {
            string getUserFriendsUri = $"/api/users/friends";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.GetAsync(getUserFriendsUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<UserGetDTO> userFriendDTOs = JsonConvert.DeserializeObject<List<UserGetDTO>>(
                await response.Content.ReadAsStringAsync());

            userFriendDTOs.Should().NotBeNull();
            userFriendDTOs.Should().HaveCount(1);

            userFriendDTOs[0].Should().BeEquivalentTo(new UserGetDTO
            {
                Email = "katarzyna@example.com",
                Username = "kate",
                AvatarImage = null,
                EmailConfirmed = false
            }, options => options.Excluding(u => u.Id));
        }

        [Theory]
        [InlineData("abc", 0)]
        [InlineData("mat", 0)]
        [InlineData("at", 1)]
        public async Task Test_Search(string query, int count)
        {
            string searchUri = $"/api/users/search?query={query}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.GetAsync(searchUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<UserGetDTO> userGetDTOs = JsonConvert.DeserializeObject<List<UserGetDTO>>(
                await response.Content.ReadAsStringAsync());

            userGetDTOs.Should().NotBeNull();
            userGetDTOs.Should().HaveCount(count);
        }

        [Fact]
        public async Task Test_GetUserMainGoal()
        {
            string getUserMainGoalUri = $"/api/users/main-goal";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.GetAsync(getUserMainGoalUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            GoalGetDTO goalGetDTO = JsonConvert.DeserializeObject<GoalGetDTO>(
                await response.Content.ReadAsStringAsync());

            goalGetDTO.Should().NotBeNull();
            goalGetDTO.Should().BeEquivalentTo(new GoalGetDTO
            {
                Percentage = 50.0,
                Amount = 20000,
                Name = "Account balance",
                GoalType = GoalTypeEnum.AccountBalance,
                CreationDate = DateTime.Parse("2024-01-11"),
                Deadline = DateTime.Parse("2024-02-01"),
                IsMain = true
            }, options => options.Excluding(g => g.Id));
        }

        [Fact]
        public async Task Test_GetUserGoals()
        {
            string getUserGoalsUri = $"/api/users/goals";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.GetAsync(getUserGoalsUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<GoalGetDTO> goalGetDTOs = JsonConvert.DeserializeObject<List<GoalGetDTO>>(
                await response.Content.ReadAsStringAsync());

            List<GoalGetDTO> expectedGoalGetDTOs = new List<GoalGetDTO>
            {
                new GoalGetDTO 
                {   
                    Percentage = 50.0,
                    Amount = 20000,
                    Name = "Account balance",
                    GoalType = GoalTypeEnum.AccountBalance,
                    CreationDate = DateTime.Parse("2024-01-11"),
                    Deadline = DateTime.Parse("2024-02-01"),
                    IsMain = true
                },

                new GoalGetDTO
                {
                    Percentage = 100,
                    Amount = 100,
                    Name = "Average expenses",
                    GoalType = GoalTypeEnum.AverageExpenses,
                    CreationDate = DateTime.Parse("2024-01-13"),
                    Deadline = DateTime.Parse("2024-02-11"),
                    IsMain = false
                }
            };

            goalGetDTOs.Should().NotBeNull();
            goalGetDTOs.Should().HaveCount(2);
            goalGetDTOs.Should().BeEquivalentTo(expectedGoalGetDTOs, 
                options => options.Excluding(g => g.Id));
        }

        public async Task InitializeAsync()
        {
            await factory.SeedDatabase();

            string userLoginUri = "/api/users/login";
            UserLoginDTO userLoginDTO = new UserLoginDTO
            {
                Email = "mateusz@example.com",
                Password = "Mati123_"
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(userLoginUri, userLoginDTO);
            token = JsonConvert.DeserializeObject<UserLoggedInDTO>(
                await response.Content.ReadAsStringAsync()).Token;

            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await databaseFixture.ResetAsync();
        }
    }
}
