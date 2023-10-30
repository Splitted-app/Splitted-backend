using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using System.Security.Cryptography;
using Splitted_backend.DbContexts;
using Splitted_backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using AuthenticationServer.Managers;

namespace Splitted_backend.Extensions
{
    public static class ServicesExtension
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(Program));
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            services.AddCors(options =>
            {
                options.AddPolicy("Allowed origins",
                    builder => builder
                    .WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>())
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        }

        public static void ConfigureDbContexts(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SplittedDbContext>(opts => opts.UseSqlServer(configuration["ConnectionStrings:SplittedDB"]));
            services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 7;
            })
                .AddEntityFrameworkStores<SplittedDbContext>()
                .AddDefaultTokenProviders();
            
            SplittedDbContext splittedDbContext = services.BuildServiceProvider().GetRequiredService<SplittedDbContext>();
            splittedDbContext.Database.Migrate();
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            LoadTokenKeys(configuration);
            AuthenticationManager authenticationManager = new AuthenticationManager(configuration);

            services.AddAuthentication(authenticationManager.ConfigureAuthenticationSchema)
                .AddJwtBearer(authenticationManager.ConfigureTokenValidation);
        }

        private static void LoadTokenKeys(IConfiguration configuration)
        {
            string? privateKeyPath = configuration["KeysPath:PrivateKeyPath"];
            string? publicKeyPath = configuration["KeysPath:PublicKeyPath"];

            if (privateKeyPath is not null && publicKeyPath is not null)
            {
                configuration["Keys:PrivateKey"] = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), 
                    privateKeyPath));
                configuration["Keys:PublicKey"] = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), 
                    publicKeyPath));
            }
        }
    }
}
