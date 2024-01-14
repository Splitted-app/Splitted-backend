using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
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

namespace AuthenticationServer.Managers
{
    public class AuthenticationManager : BaseAuthenticationManager
    {
        private IConfiguration configuration { get; set; }


        public AuthenticationManager(IConfiguration configuration) : base(configuration["Issuer"]!, 
            configuration.GetSection("Audience").Get<List<string>>())
        {
            this.configuration = configuration;
        }


        protected override SigningCredentials GetSigningCredentials()
        {
            RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(PemManager
                .LoadKey<RsaPrivateCrtKeyParameters>(configuration["Keys:PrivateKey"]!));

            return new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
        }

        protected override SecurityKey GetSigningKey()
            => new RsaSecurityKey(PemManager.LoadKey<RsaKeyParameters>(configuration["Keys:PublicKey"]!));
    }
}
