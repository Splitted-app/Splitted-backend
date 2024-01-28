using AuthenticationServer.Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SplittedIntegrationTests.Mocks
{
    public class TestAuthenticationManager : BaseAuthenticationManager
    {
        public SecurityKey securityKey { get; set; }


        public TestAuthenticationManager(IConfiguration configuration) : base("http://splitted-api.com",
            new List<string> { "http://splitted-front.com", "http://splitted-api.com" })
        {
            securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Key"]));
        }


        protected override SigningCredentials GetSigningCredentials()
            => new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        protected override SecurityKey GetSigningKey()
            => securityKey;

    }
}
