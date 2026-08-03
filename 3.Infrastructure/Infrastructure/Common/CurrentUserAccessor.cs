using Application.Common.Models;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Common
{
    /// <summary>
    /// 基于 HttpContext 中已通过 [Authorize] 验证的 JWT Claims 解析当前登录用户。
    /// 以 Scoped 生命周期注册，供 Application 层 Handler 通过构造函数注入 ICurrentUser 使用。
    /// 用户身份只来源于服务端 JWT，前台无法伪造——彻底替代原先在 Controller 里
    /// `cmd = cmd with { xxxUserid = CurrentUser.Userid }` 的样板注入。
    /// </summary>
    public class CurrentUserAccessor : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public long Userid
        {
            get
            {
                var principal = User;
                if (principal is null) return 0;
                var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? principal.FindFirstValue("sub");
                return long.TryParse(value, out var id) ? id : 0;
            }
        }

        public string Username
        {
            get
            {
                var principal = User;
                if (principal is null) return string.Empty;
                return principal.FindFirstValue(ClaimTypes.Name)
                       ?? principal.FindFirstValue("name")
                       ?? string.Empty;
            }
        }

        public string Jti
        {
            get
            {
                var principal = User;
                if (principal is null) return string.Empty;
                return principal.FindFirstValue(JwtRegisteredClaimNames.Jti)
                       ?? principal.FindFirstValue("jti")
                       ?? string.Empty;
            }
        }
    }
}
