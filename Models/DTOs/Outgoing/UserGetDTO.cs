using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing
{
    public class UserGetDTO
    {
        public string Currency { get; set; }

        public decimal BankBalance { get; set; }

        public string? Bank { get; set; }

        public string? AvatarImage { get; set; }
    }
}
