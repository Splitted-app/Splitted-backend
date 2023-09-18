using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models.CsvModels;
using Models.Enums;
using Splitted_backend.Models.Entities;

namespace Splitted_backend
{
    public class SplittedDbContext : DbContext
    {
        public SplittedDbContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions) 
        { 
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .Property(u => u.UserType)
                .HasConversion(new EnumToStringConverter<UserTypeEnum>());
        }
    }
}
