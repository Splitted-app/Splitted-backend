using FluentAssertions;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.Budget;
using Models.DTOs.Outgoing.User;
using Models.Enums;
using Newtonsoft.Json;
using SplittedIntegrationTests.CustomWebApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SplittedIntegrationTests.ControllersTests.BudgetControllerTests
{
    [Collection("WebApp Collection")]
    public class BudgetControllerDeleteTests : IAsyncLifetime
    {
        public DatabaseFixture databaseFixture { get; }

        public CustomWebApplicationFactory factory { get; }

        public HttpClient httpClient { get; }

        private string token { get; set; } = null!;


        public BudgetControllerDeleteTests(CustomWebApplicationFactory factory, DatabaseFixture databaseFixture)
        {
            this.databaseFixture = databaseFixture;
            this.factory = factory;
            this.httpClient = factory.CreateClient();
        }


        [Fact]
        public async Task Test_DeleteBudgetWithInvalidData()
        {
            string getUserBudgetsUri = "/api/users/budgets?budgetType=personal";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage budgetsReponse = await httpClient.GetAsync(getUserBudgetsUri);

            List<UserBudgetGetDTO> userBudgets = JsonConvert.DeserializeObject<List<UserBudgetGetDTO>>(
                await budgetsReponse.Content.ReadAsStringAsync());
            string budgetId = userBudgets[0].Id.ToString();

            string deleteBudgetUri = $"/api/budgets/{budgetId}";

            HttpResponseMessage response = await httpClient.DeleteAsync(deleteBudgetUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Test_DeleteBudgetWithProperData()
        {
            string getUserBudgetsUri = "/api/users/budgets?budgetType=partner";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage budgetsReponse = await httpClient.GetAsync(getUserBudgetsUri);

            List<UserBudgetGetDTO> userBudgets = JsonConvert.DeserializeObject<List<UserBudgetGetDTO>>(
                await budgetsReponse.Content.ReadAsStringAsync());
            string budgetId = userBudgets[0].Id.ToString();

            string deleteBudgetUri = $"/api/budgets/{budgetId}";

            HttpResponseMessage response = await httpClient.DeleteAsync(deleteBudgetUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
