using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Open.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Open.Filters
{
    /// <summary>
    /// 在每个 action 执行前，从已认证的 HttpContext.User 中解析出当前登录用户，
    /// 写入 BaseController.CurrentUser，供各接口直接使用。
    /// 通过 Program.cs 中 AddControllers(o => o.Filters.Add&lt;CurrentUserFilter&gt;()) 全局注册。
    /// </summary>
    public class CurrentUserFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 只对继承自 BaseController 的控制器生效
            if (context.Controller is not BaseController ctrl)
                return;

            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
                return;

            long.TryParse(
                user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub"),
                out long userid);

            ctrl.CurrentUser = new CurrentUser
            {
                Userid = userid,
                Username = user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue("name")!,
                Jti = user.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? user.FindFirstValue("jti")!
            };
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // 无需处理
        }
    }
}
