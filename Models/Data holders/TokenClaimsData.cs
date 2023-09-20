using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Data_holders
{
    public class TokenClaimsData
    {
        public Guid UserId { get; set; }

        public string Nickname { get; set; }

        public UserTypeEnum UserType { get; set; }
    }
}
