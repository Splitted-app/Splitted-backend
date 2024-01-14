using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    public abstract class BaseAuthenticationManager
    {
        private string issuer { get; set; }

        private List<string> audience { get; set; }


        public BaseAuthenticationManager(string issuer, List<string> audience)
        {
            this.issuer = issuer;
            this.audience = audience;
        }


        public string GenerateAccessToken(List<Claim> userClaims)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SigningCredentials signingCredentials = GetSigningCredentials();

            Dictionary<string, object> claimsDictionary = userClaims.ToDictionary(uc => uc.Type, uc => (object)uc.Value);
            claimsDictionary.Add(JwtRegisteredClaimNames.Aud, audience);
            claimsDictionary.Add(JwtRegisteredClaimNames.Iss, issuer);

            SecurityTokenDescriptor jwtTokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.Now.AddMinutes(10),
                SigningCredentials = signingCredentials,
                Claims = claimsDictionary
            };

            SecurityToken jwtToken = jwtSecurityTokenHandler.CreateToken(jwtTokenDescriptor);
            return jwtSecurityTokenHandler.WriteToken(jwtToken);
        }

        public string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
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
                IssuerSigningKey = GetSigningKey(),
            };
        }

        protected abstract SigningCredentials GetSigningCredentials();

        protected abstract SecurityKey GetSigningKey();
    }
}
