using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing
{
    public class UserCreatedDTO
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        public string Currency { get; set; }

        public string? Bank { get; set; }

        public decimal BankBalance { get; set; }
    }
}
