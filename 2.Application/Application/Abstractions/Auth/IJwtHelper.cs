using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Auth
{
    public interface IJwtHelper
    {
        string SecretKey { get; }
        string Issuer { get; }
        string Audience { get; }

        /// <summary>
        /// JWT密钥生成
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="username"></param>
        /// <param name="roles"></param>
        /// <param name="day"></param>
        /// <returns>返回 token,jti</returns>
        (string, string) GenerateToken(long userId, string username, string[] roles, string clientType = "WEB", int day = 7);
        /// <summary>
        /// 校验jwt
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        ClaimsPrincipal? ValidateToken(string token);
    }
}
