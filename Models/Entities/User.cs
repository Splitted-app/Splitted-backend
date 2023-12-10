using Microsoft.AspNetCore.Identity;
using Models.Entities;
using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Splitted_backend.Models.Entities
{
    [Table("Users")]
    public class User : IdentityUser<Guid>
    {
        public string? AvatarImage { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }


        public List<Budget> Budgets { get; set; } = new();

        public List<UserBudget> UserBudgets { get; set; } = new();

        public List<Transaction> Transactions { get; set; } = new();

        public List<User> Friends { get; set; } = new();

        public List<Goal> Goals { get; set; } = new();
    }
}
