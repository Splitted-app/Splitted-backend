using Microsoft.EntityFrameworkCore;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;

namespace Splitted_backend.Extensions
{
    public static class ServicesExtension
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SplittedDbContext>(opts => opts.UseSqlServer(configuration["ConnectionStrings:SplittedDB"]));
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        }
    }
}
