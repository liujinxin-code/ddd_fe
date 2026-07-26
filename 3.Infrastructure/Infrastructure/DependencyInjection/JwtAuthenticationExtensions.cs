using Application.Abstractions.Auth;
using Application.Events.User.Contracts;
using Infrastructure.Common.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DependencyInjection
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings!.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            // 匿名接口直接放行
                            var endpoint = context.HttpContext.GetEndpoint();
                            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
                                return;

                            if (context.Principal == null)
                            {
                                context.Fail("Token 无效");
                                return;
                            }

                            var jti = context.Principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                            var userIdStr = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userIdStr))
                            {
                                context.Fail("Token 无效");
                                return;
                            }

                            if (!long.TryParse(userIdStr, out var userId))
                            {
                                context.Fail("用户标识非法");
                                return;
                            }

                            var clientType = context.Principal.FindFirst("client_type")?.Value;
                            var token = context.HttpContext.Request.Headers["Authorization"]
                                .ToString()
                                .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase)
                                .Trim();

                            var tokenCache = context.HttpContext.RequestServices
                                .GetRequiredService<ITokenCacheService>();
                            var userRepo = context.HttpContext.RequestServices
                                .GetRequiredService<ITkUserRepository>();

                            if (clientType == "WEB")
                            {
                                if (!await tokenCache.GetTokenExistsAsync(jti))
                                    context.Fail("Token 已失效");
                            }
                            else if (clientType == "API")
                            {
                                if (!await userRepo.GetUserExistsByApiKey(userId, token))
                                    context.Fail("Token 无效");
                            }
                            else
                            {
                                context.Fail("未知客户端类型");
                            }
                        }
                    };

                    //    //  拦截 401 响应
                    //    OnChallenge = async context =>
                    //{
                    //    // 阻止默认返回 401
                    //    //  context.HandleResponse();
                    //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    //    context.Response.ContentType = "application/json";
                    //    var result = JsonSerializer.Serialize(ApiResult.UnAuth());
                    //    await context.Response.WriteAsync(result);
                    //},
                    //JWT role权限校验，没权限时进入
                    //OnForbidden = async context =>
                    //{
                    //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    //    context.Response.ContentType = "application/json";
                    //    var result = JsonSerializer.Serialize(ApiResult.Forbidden());
                    //    await context.Response.WriteAsync(result);
                    //}
                });

            return services;
        }
    }
}
