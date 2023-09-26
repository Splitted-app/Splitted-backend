using Microsoft.AspNetCore.Identity;
using Models.Entities;
using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Splitted_backend.Models.Entities
{
    [Table("Users")]
    public class User : IdentityUser<Guid>
    {
        public string Currency { get; set; }

        public string? Bank { get; set; }

        [Column(TypeName = "money")]
        public decimal BankBalance { get; set; }

        public string? AvatarImage { get; set; }


        public List<Transaction> Transactions { get; set; } = new();

        public List<UserTransaction> UserTransactions { get; set; } = new();
    }
}
