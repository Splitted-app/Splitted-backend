using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Splitted_backend.DbContexts;

namespace SplittedIntegrationTests.CustomWebApp
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private Respawner respawner { get; set; } = null!;

        private IConfiguration configuration { get; }


        public DatabaseFixture()
        {
            this.configuration = new ConfigurationBuilder()
                .AddJsonFile(CustomWebApplicationFactory.testSettingsPath)
                .Build();
        }


        public async Task InitializeAsync()
        {
            respawner = await Respawner.CreateAsync(configuration["ConnectionStrings:SplittedDB"]!, new RespawnerOptions
            {
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" },
                SchemasToInclude = new string[] { "dbo" },
                WithReseed = true,
            }); 
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask; 
        }

        public async Task ResetAsync()
        {
            await respawner.ResetAsync(configuration["ConnectionStrings:SplittedDB"]!);
        }
    }
}
