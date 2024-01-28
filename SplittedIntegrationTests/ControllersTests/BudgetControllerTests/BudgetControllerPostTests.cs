using FluentAssertions;
using Models.DTOs.Incoming.User;
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
using Models.Enums;
using Models.DTOs.Outgoing.Budget;
using Xunit.Abstractions;
using Models.DTOs.Outgoing.Transaction;
using Models.DTOs.Incoming.Budget;
using Models.DTOs.Incoming.Transaction;
using Models.DTOs.Outgoing.TransactionPayBack;

namespace SplittedIntegrationTests.ControllersTests.BudgetControllerTests
{
    [Collection("WebApp Collection")]
    public class BudgetControllerPostTests : IAsyncLifetime
    {
        public DatabaseFixture databaseFixture { get; }

        public CustomWebApplicationFactory factory { get; }

        public HttpClient httpClient { get; }

        private string token { get; set; } = null!;


        public BudgetControllerPostTests(CustomWebApplicationFactory factory, DatabaseFixture databaseFixture)
        {
            this.databaseFixture = databaseFixture;
            this.factory = factory;
            this.httpClient = factory.CreateClient();
        }


        [Fact]
        public async Task Test_CreateBudgetWithInvalidData()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string budgetPostUri = "/api/budgets";
            BudgetPostDTO budgetPostDTO = new BudgetPostDTO
            {
                Bank = BankNameEnum.Santander,
                Currency = "PLN",
                BudgetBalance = 10000.50M,
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(budgetPostUri, budgetPostDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Test_CreateBudgetWithProperData()
        {
            string userLoginUri = "/api/users/login";
            UserLoginDTO userLoginDTO = new UserLoginDTO
            {
                Email = "user@example.com",
                Password = "User123_"
            };

            HttpResponseMessage tokenResponse = await httpClient.PostAsJsonAsync(userLoginUri, userLoginDTO);
            string userToken = JsonConvert.DeserializeObject<UserLoggedInDTO>(
                await tokenResponse.Content.ReadAsStringAsync()).Token;

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            string budgetPostUri = "/api/budgets";
            BudgetPostDTO budgetPostDTO = new BudgetPostDTO
            {
                Bank = BankNameEnum.Santander,
                Currency = "PLN",
                BudgetBalance = 10000.50M,
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(budgetPostUri, budgetPostDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            BudgetCreatedDTO budgetCreatedDTO = JsonConvert.DeserializeObject<BudgetCreatedDTO>(
                await response.Content.ReadAsStringAsync());

            budgetCreatedDTO.Should().NotBeNull();
            budgetCreatedDTO.Should().BeEquivalentTo(new BudgetCreatedDTO
            {
                Bank = BankNameEnum.Santander,
                Currency = "PLN",
                BudgetBalance = 10000.50M,
                BudgetType = BudgetTypeEnum.Personal,
                Name = string.Empty
            }, options => options.Excluding(b => b.Id));
        }

        [Theory]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/IngCsv/IngTest4.csv", BankNameEnum.Mbank, false, 
            HttpStatusCode.BadRequest)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/MbankCsv/MbankTest4.csv", BankNameEnum.Pekao, false, 
            HttpStatusCode.BadRequest)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/PkoCsv/PkoTest4.csv", BankNameEnum.Ing, false, 
            HttpStatusCode.BadRequest)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/PekaoCsv/PekaoTest4.csv", BankNameEnum.Santander, false, 
            HttpStatusCode.BadRequest)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/SantanderCsv/SantanderTest4.csv",
            BankNameEnum.Pko, false, HttpStatusCode.BadRequest)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/IngCsv/IngBroken4.jpg",
            BankNameEnum.Santander, false, HttpStatusCode.BadRequest)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/SantanderCsv/SantanderTest3.csv",
            BankNameEnum.Santander, true, HttpStatusCode.NotFound)]

        public async Task Test_UploadCsvFileWithInvalidData(string filePath, BankNameEnum bank, bool wrongBudget, 
            HttpStatusCode statusCode)
        {
            string getUserBudgetsUri = $"/api/users/budgets";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage getResponse = await httpClient.GetAsync(getUserBudgetsUri);

            List<UserBudgetGetDTO> userBudgetGetDTOs = JsonConvert.DeserializeObject<List<UserBudgetGetDTO>>(
                await getResponse.Content.ReadAsStringAsync());

            string budgetId = wrongBudget ? Guid.NewGuid().ToString() : userBudgetGetDTOs[0].Id.ToString();
            string budgetCsvPostUri = $"/api/budgets/{budgetId}/transactions/csv?bank={bank}";

            MultipartFormDataContent multiForm = new MultipartFormDataContent();

            FileStream fileStream = File.OpenRead(filePath);
            StreamContent streamContent = new StreamContent(fileStream);

            streamContent.Headers.Add("Content-Type", "text/csv");
            multiForm.Add(streamContent, "csvFile", Path.GetFileName(filePath));

            HttpResponseMessage response = await httpClient.PostAsync(budgetCsvPostUri, multiForm);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(statusCode);
        }

        [Theory]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/IngCsv/IngTest4.csv", BankNameEnum.Ing, 21)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/MbankCsv/MbankTest4.csv", BankNameEnum.Mbank, 21)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/PkoCsv/PkoTest4.csv", BankNameEnum.Pko, 15)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/PekaoCsv/PekaoTest4.csv", BankNameEnum.Pekao, 19)]
        [InlineData("../../../../SplittedUnitTests/Data/FakeCsvData/SantanderCsv/SantanderTest4.csv", 
            BankNameEnum.Santander, 17)]
        public async Task Test_UploadCsvFileWithProperData(string filePath, BankNameEnum bank, int count)
        {
            string getUserBudgetsUri = $"/api/users/budgets";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage getResponse = await httpClient.GetAsync(getUserBudgetsUri);

            List<UserBudgetGetDTO> userBudgetGetDTOs = JsonConvert.DeserializeObject<List<UserBudgetGetDTO>>(
                await getResponse.Content.ReadAsStringAsync());

            string budgetId = userBudgetGetDTOs[0].Id.ToString();
            string budgetCsvPostUri = $"/api/budgets/{budgetId}/transactions/csv?bank={bank}";

            MultipartFormDataContent multiForm = new MultipartFormDataContent();

            FileStream fileStream = File.OpenRead(filePath);
            StreamContent streamContent = new StreamContent(fileStream);

            streamContent.Headers.Add("Content-Type", "text/csv");
            multiForm.Add(streamContent, "csvFile", Path.GetFileName(filePath));

            HttpResponseMessage response = await httpClient.PostAsync(budgetCsvPostUri, multiForm);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            List<TransactionGetDTO> transactionGetDTOs = JsonConvert.DeserializeObject<List<TransactionGetDTO>>(
                await response.Content.ReadAsStringAsync());

            transactionGetDTOs.Should().NotBeNull();
            transactionGetDTOs.Should().HaveCount(count);
        }

        [Theory]
        [InlineData(false, HttpStatusCode.Forbidden)]
        [InlineData(true, HttpStatusCode.NotFound)]
        public async Task Test_PostTransactionToBudgetWithInvalidData(bool wrongBudget, HttpStatusCode statusCode)
        {
            string userLoginUri = "/api/users/login";
            UserLoginDTO userLoginDTO = new UserLoginDTO
            {
                Email = "user@example.com",
                Password = "User123_"
            };

            HttpResponseMessage tokenResponse = await httpClient.PostAsJsonAsync(userLoginUri, userLoginDTO);
            string userToken = JsonConvert.DeserializeObject<UserLoggedInDTO>(
                await tokenResponse.Content.ReadAsStringAsync()).Token;

            string getUserBudgetsUri = $"/api/users/budgets";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage getResponse = await httpClient.GetAsync(getUserBudgetsUri);

            List<UserBudgetGetDTO> userBudgetGetDTOs = JsonConvert.DeserializeObject<List<UserBudgetGetDTO>>(
                await getResponse.Content.ReadAsStringAsync());
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            string budgetId = wrongBudget ? Guid.NewGuid().ToString() : userBudgetGetDTOs[0].Id.ToString();
            string transactionBudgetPostUri = $"/api/budgets/{budgetId}/transactions";

            TransactionPostDTO transactionPostDTO = new TransactionPostDTO
            {
                Amount = -567,
                Currency = "PLN",
                Date = DateTime.Parse("2024-01-01"),
                Description = "Test transaction",
                TransactionType = TransactionTypeEnum.Other,
                UserCategory = "Food"
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(transactionBudgetPostUri, 
                transactionPostDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(statusCode);
        }

        [Fact]
        public async Task Test_PostTransactionToBudgetWithProperData()
        {
            string getUserBudgetsUri = $"/api/users/budgets";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage getResponse = await httpClient.GetAsync(getUserBudgetsUri);

            List<UserBudgetGetDTO> userBudgetGetDTOs = JsonConvert.DeserializeObject<List<UserBudgetGetDTO>>(
                await getResponse.Content.ReadAsStringAsync());

            string budgetId = userBudgetGetDTOs[0].Id.ToString();
            string transactionBudgetPostUri = $"/api/budgets/{budgetId}/transactions";

            TransactionPostDTO transactionPostDTO = new TransactionPostDTO
            {
                Amount = -567,
                Currency = "PLN",
                Date = DateTime.Parse("2024-01-01"),
                Description = "Test transaction",
                TransactionType = TransactionTypeEnum.Other,
                UserCategory = "Food"
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(transactionBudgetPostUri,
                transactionPostDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            TransactionGetDTO transactionGetDTO = JsonConvert.DeserializeObject<TransactionGetDTO>(
                await response.Content.ReadAsStringAsync());

            transactionGetDTO.Should().NotBeNull();
            transactionGetDTO.Should().BeEquivalentTo(new TransactionGetDTO
            {
                Amount = -567,
                Currency = "PLN",
                Date = DateTime.Parse("2024-01-01"),
                Description = "Test transaction",
                TransactionType = TransactionTypeEnum.Other,
                UserCategory = "Food",
                UserName = "mati",
            }, options => options.Excluding(t => t.Id));
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
