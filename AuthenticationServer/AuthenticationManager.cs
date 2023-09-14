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
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationServer
{
    public class AuthenticationManager
    {
        private IConfiguration configuration { get; set; }


        public AuthenticationManager(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GenerateToken()
        { 
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(PemManager.LoadKey<RsaPrivateCrtKeyParameters>(configuration["Keys:PrivateKey"]!));
            SigningCredentials signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

            SecurityTokenDescriptor jwtTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, "test")}),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = signingCredentials
            };

            SecurityToken jwtToken = jwtSecurityTokenHandler.CreateToken(jwtTokenDescriptor);
            return jwtSecurityTokenHandler.WriteToken(jwtToken);
        }

        public void ConfigureAuthenticationSchema(AuthenticationOptions options)
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }

        public void ConfigureTokenValidation(JwtBearerOptions options)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "sadf", // from appsettings
                ValidAudience = "sdf", // from appsettings
                IssuerSigningKey = new RsaSecurityKey(PemManager.LoadKey<RsaKeyParameters>(configuration["Keys:PublicKey"]!))
            };
        }
    }
}
