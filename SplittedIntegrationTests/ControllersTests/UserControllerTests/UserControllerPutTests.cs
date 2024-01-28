using FluentAssertions;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.User;
using Newtonsoft.Json;
using SplittedIntegrationTests.CustomWebApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SplittedIntegrationTests.ControllersTests.UserControllerTests
{
    [Collection("WebApp Collection")]
    public class UserControllerPutTests : IAsyncLifetime
    {
        public DatabaseFixture databaseFixture { get; }

        public CustomWebApplicationFactory factory { get; }

        public HttpClient httpClient { get; }

        private string token { get; set; } = null!;


        public UserControllerPutTests(CustomWebApplicationFactory factory, DatabaseFixture databaseFixture)
        {
            this.databaseFixture = databaseFixture;
            this.factory = factory;
            this.httpClient = factory.CreateClient();
        }


        [Fact]
        public async Task Test_PutUserWithInvalidData()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string putUserUri = "/api/users";
            UserPutDTO userLoginDTO = new UserPutDTO
            {
                UserName = "kate",
            };

            HttpResponseMessage response = await httpClient.PutAsJsonAsync(putUserUri, userLoginDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Test_PutUserWithProperData()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string putUserUri = "/api/users";
            UserPutDTO userLoginDTO = new UserPutDTO
            {
                AvatarImage = "https://test-profile-picture.com",
                UserName = "mati1",
            };

            HttpResponseMessage response = await httpClient.PutAsJsonAsync(putUserUri, userLoginDTO);

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
