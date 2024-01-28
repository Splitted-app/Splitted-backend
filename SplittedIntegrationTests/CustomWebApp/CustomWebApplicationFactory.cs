using AuthenticationServer.Managers;
using CsvHelper;
using ExternalServices.StorageClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models.Entities;
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
using System.Runtime.CompilerServices;
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

                services.AddSingleton(_ => PythonExecuterMock.GetMockedPythonExecuter());

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

                User firstUser = new User
                {
                    Email = "mateusz@example.com",
                    UserName = "mati",
                };
                await AddUser(firstUser, "Mati123_", userManager);

                User secondUser = new User
                {
                    Email = "katarzyna@example.com",
                    UserName = "kate",
                };
                await AddUser(secondUser, "Kate123_", userManager);

                User thirdUser = new User
                {
                    Email = "user@example.com",
                    UserName = "user",
                };
                await AddUser(thirdUser, "User123_", userManager);

                firstUser.Friends.Add(secondUser);
                secondUser.Friends.Add(firstUser);

                Budget firstBudget = new Budget
                {
                    Bank = BankNameEnum.Ing,
                    BudgetType = BudgetTypeEnum.Personal,
                    Name = string.Empty,
                    Currency = "PLN",
                    BudgetBalance = 10000,
                    CreationDate = DateTime.Parse("2024-01-02")
                };
                AddBudget(firstBudget, new List<User> { firstUser }, repositoryWrapper);

                Budget secondBudget = new Budget
                {
                    Bank = BankNameEnum.Mbank,
                    BudgetType = BudgetTypeEnum.Personal,
                    Name = string.Empty,
                    Currency = "PLN",
                    BudgetBalance = 12456,
                    CreationDate = DateTime.Parse("2024-01-04")
                };
                AddBudget(secondBudget, new List<User> { secondUser }, repositoryWrapper);

                Budget thirdBudget = new Budget
                {
                    BudgetType = BudgetTypeEnum.Partner,
                    Name = "Partner",
                    Currency = "PLN",
                    CreationDate = DateTime.Parse("2024-01-11")
                };
                AddBudget(thirdBudget, new List<User> { firstUser, secondUser }, repositoryWrapper);

                Goal firstGoal = new Goal
                {
                    Amount = 20000,
                    Name = "Account balance",
                    GoalType = GoalTypeEnum.AccountBalance,
                    CreationDate = DateTime.Parse("2024-01-11"),
                    Deadline = DateTime.Parse("2024-02-01"),
                    IsMain = true
                };
                AddGoal(firstGoal, firstUser, repositoryWrapper);

                Goal secondGoal = new Goal
                {
                    Amount = 100,
                    Name = "Average expenses",
                    GoalType = GoalTypeEnum.AverageExpenses,
                    CreationDate = DateTime.Parse("2024-01-13"),
                    Deadline = DateTime.Parse("2024-02-11"),
                    IsMain = false
                };
                AddGoal(secondGoal, firstUser, repositoryWrapper);

                Transaction firstTransaction = new Transaction
                {
                    Amount = -150,
                    Currency = "PLN",
                    Date = DateTime.Parse("2024-01-01"),
                    Description = "Transaction 1",
                    AutoCategory = "Food",
                    UserCategory = "Food",
                };
                AddTransaction(firstTransaction, firstUser, firstBudget, repositoryWrapper);

                Transaction secondTransaction = new Transaction
                {
                    Amount = -50.5M,
                    Currency = "PLN",
                    Date = DateTime.Parse("2024-01-02"),
                    Description = "Transaction 2",
                    AutoCategory = "Clothes",
                    UserCategory = "Clothes",
                };
                AddTransaction(secondTransaction, firstUser, firstBudget, repositoryWrapper);

                await repositoryWrapper.SaveChanges();
            }
        }

        private async Task AddUser(User user, string password, UserManager<User> userManager)
        {
            await userManager.CreateAsync(user, password);
            await userManager.AddUserClaims(user);
        }

        private void AddBudget(Budget budget, List<User> users, IRepositoryWrapper repositoryWrapper)
        {
            repositoryWrapper.Budgets.Create(budget);
            users.ForEach(u => u.Budgets.Add(budget));
        }

        private void AddGoal(Goal goal, User user, IRepositoryWrapper repositoryWrapper)
        {
            repositoryWrapper.Goals.Create(goal);
            user.Goals.Add(goal);
        }

        private void AddTransaction(Transaction transaction, User user, Budget budget, 
            IRepositoryWrapper repositoryWrapper)
        {
            repositoryWrapper.Transactions.Create(transaction);
            user.Transactions.Add(transaction);
            budget.Transactions.Add(transaction);
        }
    }
}
