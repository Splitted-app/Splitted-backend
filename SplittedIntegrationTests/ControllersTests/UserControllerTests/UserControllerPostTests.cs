using FluentAssertions;
using Models.DTOs.Incoming.User;
using Models.DTOs.Outgoing.Budget;
using Models.DTOs.Outgoing.Goal;
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
using Models.DTOs.Incoming.Goal;
using Xunit.Abstractions;

namespace SplittedIntegrationTests.ControllersTests.UserControllerTests
{
    [Collection("WebApp Collection")]
    public class UserControllerPostTests : IAsyncLifetime
    {
        public DatabaseFixture databaseFixture { get; }

        public CustomWebApplicationFactory factory { get; }

        public HttpClient httpClient { get; }

        private string token { get; set; } = null!;


        public UserControllerPostTests(CustomWebApplicationFactory factory, DatabaseFixture databaseFixture)
        {
            this.databaseFixture = databaseFixture;
            this.factory = factory;
            this.httpClient = factory.CreateClient();
        }


        [Theory]
        [InlineData(null, "Haslo123_", "nowy", HttpStatusCode.BadRequest)]
        [InlineData("nowy@example.com", "Haslo1", "nowy", HttpStatusCode.BadRequest)]
        [InlineData("mateusz@example.com", "Haslo123_", "nowy", HttpStatusCode.Conflict)]
        [InlineData("nowy@example.com", "Haslo123_", "mati", HttpStatusCode.Conflict)]
        public async Task Test_RegisterUserWithInvalidData(string? email, string password, string userName, 
            HttpStatusCode httpStatusCode)
        {
            string userRegisterUri = "/api/users/register";
            UserRegisterDTO userRegisterDTO = new UserRegisterDTO
            {
                Email = email,
                Password = password,
                UserName = userName,
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(userRegisterUri, userRegisterDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(httpStatusCode);
        }

        [Fact]
        public async Task Test_RegisterUserWithProperData()
        {
            string userRegisterUri = "/api/users/register";
            UserRegisterDTO userRegisterDTO = new UserRegisterDTO
            {
                Email = "nowy@example.com",
                Password = "Haslo123_",
                UserName = "nowy",
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(userRegisterUri, userRegisterDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            UserCreatedDTO userCreatedDTO = JsonConvert.DeserializeObject<UserCreatedDTO>(
                await response.Content.ReadAsStringAsync());

            userCreatedDTO.Should().NotBeNull();
            userCreatedDTO.Should().BeEquivalentTo(new UserCreatedDTO
            {
                Email = "nowy@example.com",
                Username = "nowy"
            }, options => options.Excluding(g => g.Id));
        }

        [Theory]
        [InlineData(null, "Haslo123_", HttpStatusCode.BadRequest)]
        [InlineData("nowy1@example.com", "Haslo123_", HttpStatusCode.NotFound)]
        [InlineData("mateusz@example.com", "Haslo123_12", HttpStatusCode.Unauthorized)]
        public async Task Test_LoginUserWithInvalidData(string? email, string password, HttpStatusCode httpStatusCode)
        {
            string userLoginUri = "/api/users/login";
            UserLoginDTO userLoginDTO = new UserLoginDTO
            {
                Email = email,
                Password = password,
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(userLoginUri, userLoginDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(httpStatusCode);
        }

        [Fact]
        public async Task Test_LoginUserWithProperData()
        {
            string userLoginUri = "/api/users/login";
            UserLoginDTO userLoginDTO = new UserLoginDTO
            {
                Email = "katarzyna@example.com",
                Password = "Kate123_",
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(userLoginUri, userLoginDTO);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            UserLoggedInDTO userLoggedInDTO = JsonConvert.DeserializeObject<UserLoggedInDTO>(
                await response.Content.ReadAsStringAsync());

            userLoggedInDTO.Should().NotBeNull();
            userLoggedInDTO.Token.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_RevokeToken()
        {
            string userRevokeTokenUri = "/api/users/revoke";
           
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.PostAsync(userRevokeTokenUri, null);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("21095799-2571-413d-b22b-4b0785f1b455", HttpStatusCode.NotFound)]
        [InlineData(null, HttpStatusCode.BadRequest)]
        public async Task Test_AddFriendWithInvalidData(string? friendId, HttpStatusCode httpStatusCode)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (friendId is null)
            {
                string getUserFriendsUri = $"/api/users/friends";

                HttpResponseMessage getResponse = await httpClient.GetAsync(getUserFriendsUri);

                List<UserGetDTO> userFriendDTOs = JsonConvert.DeserializeObject<List<UserGetDTO>>(
                    await getResponse.Content.ReadAsStringAsync());

                friendId = userFriendDTOs[0].Id.ToString();
            }

            string userAddFriendUri = $"/api/users/friends/{friendId}";

            HttpResponseMessage response = await httpClient.PostAsync(userAddFriendUri, null);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(httpStatusCode);
        }

        [Fact]
        public async Task Test_AddFriendWithProperData()
        {
            string getUserFriendsUri = $"/api/users/search?query=user";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage getResponse = await httpClient.GetAsync(getUserFriendsUri);

            List<UserGetDTO> foundUsersDTOs = JsonConvert.DeserializeObject<List<UserGetDTO>>(
                await getResponse.Content.ReadAsStringAsync());

            string friendId = foundUsersDTOs[0].Id.ToString();
            string userAddFriendUri = $"/api/users/friends/{friendId}";

            HttpResponseMessage response = await httpClient.PostAsync(userAddFriendUri, null);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
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
