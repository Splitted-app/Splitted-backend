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

        private string issuer { get; set; }

        private List<string> audience { get; set; }


        public AuthenticationManager(IConfiguration configuration)
        {
            this.configuration = configuration;
            issuer = configuration["Issuer"]!;
            audience = configuration.GetSection("Audience").Get<List<string>>();
        }

        public string GenerateToken()
        { 
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(PemManager.LoadKey<RsaPrivateCrtKeyParameters>(configuration["Keys:PrivateKey"]!));
            SigningCredentials signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

            SecurityTokenDescriptor jwtTokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = signingCredentials,
                Issuer = issuer,
                Claims = new Dictionary<string, object>
                {
                    {JwtRegisteredClaimNames.Aud,  audience}
                }
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
                ValidIssuer = issuer,
                ValidAudiences = audience,
                IssuerSigningKey = new RsaSecurityKey(PemManager.LoadKey<RsaKeyParameters>(configuration["Keys:PublicKey"]!))
            };
        }
    }
}
