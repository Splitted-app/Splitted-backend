using Microsoft.EntityFrameworkCore;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using Splitted_backend.DbContexts;
using Splitted_backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using AuthenticationServer.Managers;
using Models.EmailModels;
using ExternalServices.EmailSender;
using AIService;
using ExternalServices.StorageClient;
using Splitted_backend.Utils.TimeProvider;

namespace Splitted_backend.Extensions
{
    public static class ServicesExtension
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(Program));

            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IStorageClient, StorageClient>();
            services.AddScoped<ITimeProvider, TimeProvider>();

            services.AddSingleton(configuration
                .GetSection("emailConfiguration")
                .Get<EmailConfiguration>());
            services.AddSingleton<PythonExecuter>();

            services.AddCors(options =>
            {
                options.AddPolicy("Allowed origins",
                    builder => builder
                    .WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>())
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
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
