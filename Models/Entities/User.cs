using Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Splitted_backend.Models.Entities
{
    [Table("User")]
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Nickname { get; set; }

        public string Currency { get; set; }

        public string? Bank { get; set; }

        public decimal? BankBalance { get; set; }

        public string? AvatarImage { get; set; }

        public UserTypeEnum UserType { get; set; }
    }
}
