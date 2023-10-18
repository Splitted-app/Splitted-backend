using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.User
{
    public class UserGetDTO
    {
        public string Email { get; set; }

        public string Username { get; set; }

        public string? AvatarImage { get; set; }
    }
}
