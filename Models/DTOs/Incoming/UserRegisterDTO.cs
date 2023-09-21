using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Incoming
{
    public class UserRegisterDTO
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Nickname { get; set; }

        [Required]
        public string Currency { get; set; }

        public decimal? BankBalance { get; set; }

        public string? Bank { get; set; }

        public string? AvatarImage { get; set; }
    }
}
