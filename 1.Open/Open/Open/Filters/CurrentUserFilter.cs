using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Open.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Open.Filters
{
    public class CurrentUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // 只对继承自 BaseController 的控制器生效
            if (context.Controller is not BaseController ctrl)
            {
                await next();
                return;
            }

            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            long.TryParse(
                user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue("sub"),
                out long userId);

            ctrl.CurrentUser = new CurrentUser
            {
                Userid = userId,
                Username = user.FindFirstValue(ClaimTypes.Name) ??
                           user.FindFirstValue("name")!,
                Jti = user.FindFirstValue(JwtRegisteredClaimNames.Jti) ??
                      user.FindFirstValue("jti")!
            };

            // 继续执行后续 pipeline
            await next();
        }
    }
}
