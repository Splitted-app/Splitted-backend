using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models.CsvModels;
using Models.Entities;
using Models.Enums;
using Splitted_backend.Models.Entities;

namespace Splitted_backend.DbContexts
{
    public class SplittedDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Budget> Budgets { get; set; }


        public SplittedDbContext(DbContextOptions<SplittedDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

            modelBuilder.Entity<User>()
                .HasMany(u => u.Budgets)
                .WithMany(b => b.Users)
                .UsingEntity<UserBudget>();
            modelBuilder.Entity<User>()
                .HasMany(u => u.Friends)
                .WithMany();
            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransactionType)
                .HasConversion(new EnumToStringConverter<TransactionTypeEnum>());
            modelBuilder.Entity<Budget>()
                .Property(b => b.Bank)
                .HasConversion(new EnumToStringConverter<BankNameEnum>());
            modelBuilder.Entity<Budget>()
                .Property(b => b.BudgetType)
                .HasConversion(new EnumToStringConverter<BudgetTypeEnum>());
        }
    }
}
