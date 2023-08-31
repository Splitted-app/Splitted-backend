using Microsoft.EntityFrameworkCore;
using Models.CsvModels;
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
    }
}
