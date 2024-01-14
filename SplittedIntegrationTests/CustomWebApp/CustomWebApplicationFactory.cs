using AuthenticationServer.Managers;
using CsvHelper;
using ExternalServices.StorageClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models.Enums;
using Splitted_backend.DbContexts;
using Splitted_backend.Extensions;
using Splitted_backend.Interfaces;
using Splitted_backend.Managers;
using Splitted_backend.Models.Entities;
using SplittedIntegrationTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedIntegrationTests.CustomWebApp
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public static string testSettingsPath { get; } = Path.Combine(Directory.GetCurrentDirectory(),
            "../../../Settings/appsettings.Test.json");


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(testSettingsPath)
                .Build();

            builder.UseConfiguration(configuration);
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => StorageClientMock.GetMockedStorageClient());
                services.AddScoped(_ => EmailSenderMock.GetMockedEmailSender());
                services.AddScoped<BaseAuthenticationManager, TestAuthenticationManager>();

                BaseAuthenticationManager authenticationManager = services.BuildServiceProvider()
                    .GetRequiredService<BaseAuthenticationManager>();

                services.AddAuthentication(authenticationManager.ConfigureAuthenticationSchema)
                    .AddJwtBearer(authenticationManager.ConfigureTokenValidation);
            });
        }

        public async Task SeedDatabase()
        {
            using (IServiceScope scope = Services.CreateScope())
            {
                IRepositoryWrapper repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

                UserManager<User> userManager = (UserManager<User>)scope.ServiceProvider
                    .GetService(typeof(UserManager<User>))!;
                RoleManager<IdentityRole<Guid>> roleManager = (RoleManager<IdentityRole<Guid>>)scope.ServiceProvider
                    .GetService(typeof(RoleManager<IdentityRole<Guid>>))!;

                User user = new User
                {
                    Email = "mateusz@example.com",
                    UserName = "mati",
                };

                await userManager.CreateAsync(user, "Mati123_");
                await userManager.AddUserRoles(roleManager, user, new List<UserRoleEnum> { UserRoleEnum.Member });
                await userManager.AddUserClaims(user);
            }
        }
    }
}
