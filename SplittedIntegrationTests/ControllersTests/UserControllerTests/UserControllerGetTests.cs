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

namespace SplittedIntegrationTests.ControllersTests.UserControllerTests
{
    public class UserControllerGetTests
    {
        public ITestOutputHelper output { get; set; }

        private HttpClient httpClient { get; }

        public UserControllerGetTests(ITestOutputHelper output)
        {
            httpClient = new CustomWebApplicationFactory().CreateClient();
            this.output = output;
        }


        [Fact]
        public async Task Test_CheckUserEmail() 
        {
            string userEmail = "user@example.com";
            string checkEmailRequestUri = $"/api/users/email-check?email={userEmail}";

            HttpResponseMessage response = await httpClient.GetAsync(checkEmailRequestUri);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            UserEmailCheckDTO userEmailCheckDTO = JsonConvert.DeserializeObject<UserEmailCheckDTO>(
                await response.Content.ReadAsStringAsync());

            userEmailCheckDTO.Should().NotBeNull();
            userEmailCheckDTO.Should().BeEquivalentTo(new UserEmailCheckDTO
            {
                UserExists = false
            });
        }
    }
}
