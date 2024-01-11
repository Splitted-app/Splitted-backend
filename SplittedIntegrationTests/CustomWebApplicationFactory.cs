using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedIntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private string testSettingsPath { get; } = Path.Combine(Directory.GetCurrentDirectory(),
            "../../../Settings/appsettings.Test.json");


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(testSettingsPath)
                .Build();

            builder.UseConfiguration(configuration);
        }

    }
}
