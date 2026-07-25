using Application.Abstractions.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.Auth
{
    public class JwtHelper : IJwtHelper
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; }
        /// <summary>
        /// 来源
        /// </summary>
        public string Issuer { get; }
        /// <summary>
        /// 
        /// </summary>
        public string Audience { get; }

        public JwtHelper(IOptions<JwtOptions> options)
        {
            SecretKey = options.Value.SecretKey;
            Issuer = options.Value.Issuer;
            Audience = options.Value.Audience;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="username"></param>
        /// <param name="roles"></param>
        /// <param name="day"></param>
        /// <returns>返回 token,jti</returns>
        public (string, string) GenerateToken(long userId, string username, string[] roles, string clientType = "WEB", int day = 7)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub,userId.ToString() ),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
              new Claim("client_type", clientType),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
        };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(day),
                signingCredentials: credentials
            );
            return (new JwtSecurityTokenHandler().WriteToken(token), jti);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Issuer,

                    ValidateAudience = true,
                    ValidAudience = Audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
