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

namespace SplittedIntegrationTests.ControllersTests.UserControllerTests
{
    public class UserControllerGetTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<DatabaseFixture>,
        IAsyncLifetime
    {
        public ITestOutputHelper output { get; }

        public DatabaseFixture databaseFixture { get; }

        public CustomWebApplicationFactory factory { get; }

        public HttpClient httpClient { get; }

        private string token { get; set; } = null!;


        public UserControllerGetTests(CustomWebApplicationFactory factory, 
            DatabaseFixture databaseFixture, ITestOutputHelper output) 
        {
            this.output = output;
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
