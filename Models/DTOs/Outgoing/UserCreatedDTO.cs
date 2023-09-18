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

        public string Mail { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Nickname { get; set; }

        public string? Bank { get; set; }
    }
}
