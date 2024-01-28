using FluentAssertions;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.Budget;
using Models.DTOs.Outgoing.User;
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
using Models.DTOs.Incoming.Budget;
using Models.Enums;
using Xunit.Abstractions;

namespace SplittedIntegrationTests.ControllersTests.BudgetControllerTests
{
    [Collection("WebApp Collection")]
    public class BudgetControllerPutTests : IAsyncLifetime
    {
        public DatabaseFixture databaseFixture { get; }

        public CustomWebApplicationFactory factory { get; }

        public HttpClient httpClient { get; }

        private string token { get; set; } = null!;

        public ITestOutputHelper ff { get; set; }


        public BudgetControllerPutTests(CustomWebApplicationFactory factory, DatabaseFixture databaseFixture, 
            ITestOutputHelper ff)
        {
            this.ff = ff;
            this.databaseFixture = databaseFixture;
            this.factory = factory;
            this.httpClient = factory.CreateClient();
        }


        [Fact]
        public async Task Test_PutBudgetWithInvalidData()
        {
            string budgetId = "c8a3b13a-f4f9-4a3e-8a50-5d2d161fd029";
            BudgetPutDTO? budgetPutDTO = new BudgetPutDTO();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string putBudgetUri = $"/api/budgets/{budgetId}";

            HttpResponseMessage response = await httpClient.PutAsJsonAsync(putBudgetUri, budgetPutDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Test_PutBudgetWithProperData()
        {
            string getUserBudgetsUri = "/api/users/budgets?budgetType=personal";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage budgetsReponse = await httpClient.GetAsync(getUserBudgetsUri);

            List<UserBudgetGetDTO> userBudgets = JsonConvert.DeserializeObject<List<UserBudgetGetDTO>>(
                await budgetsReponse.Content.ReadAsStringAsync());
            string budgetId = userBudgets[0].Id.ToString();

            string putBudgetUri = $"/api/budgets/{budgetId}";
            BudgetPutDTO budgetPutDTO = new BudgetPutDTO
            {
                Bank = BankNameEnum.Pekao,
                Name = string.Empty
            };

            HttpResponseMessage response = await httpClient.PutAsJsonAsync(putBudgetUri, budgetPutDTO);

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
