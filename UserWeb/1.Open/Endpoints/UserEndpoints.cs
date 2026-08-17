using Application.Common.Models;
using Application.Features.User.Models;
using Application.Features.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Open.Endpoints;

/// <summary>
/// User 领域端点（Minimal API 风格）。
/// 等价于原 MVC 风格的 UserController，路由前缀统一为 /api/user。
/// 注册/登录匿名，其余接口需 JWT（或 API Key）鉴权。
/// </summary>
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user")
            .WithTags("User");

        // 注册（匿名）
        group.MapPost("/register", async (RegisterCommand cmd, IMediator mediator, CancellationToken ct) =>
                await mediator.Send(cmd, ct))
            .AllowAnonymous()
            .Produces<ApiResult>(StatusCodes.Status200OK)
            .WithName("UserRegister")
            .WithDescription("用户注册");

        // 登录（匿名，返回 JWT）
        group.MapPost("/login", async (LoginQuery cmd, IMediator mediator, CancellationToken ct) =>
                await mediator.Send(cmd, ct))
            .AllowAnonymous()
            .Produces<ApiResult<LoginResponse>>(StatusCodes.Status200OK)
            .WithName("UserLogin")
            .WithDescription("用户登录，返回 JWT");

        // 退出登录（需鉴权，注销 Redis jti）
        group.MapGet("/logout", async (IMediator mediator) =>
                await mediator.Send(new LogoutCommand()))
            .RequireAuthorization()
            .WithName("UserLogout")
            .WithDescription("退出登录");

        // 个人信息（需鉴权）
        group.MapGet("/info", async (IMediator mediator) =>
                await mediator.Send(new GetUserInfoQuery()))
            .RequireAuthorization()
            .Produces<ApiResult<UserInfoResponse>>(StatusCodes.Status200OK)
            .WithName("UserInfo")
            .WithDescription("获取当前登录用户个人信息");

        // 修改密码（需鉴权，校验原密码）
        group.MapPost("/change-password", async (ChangePasswordCommand cmd, IMediator mediator, CancellationToken ct) =>
                await mediator.Send(cmd, ct))
            .RequireAuthorization()
            .Produces<ApiResult>(StatusCodes.Status200OK)
            .WithName("UserChangePassword")
            .WithDescription("修改当前登录用户密码");

        // 生成/刷新 API Key（需鉴权，直接生成 20 年期 JWT 覆盖旧 Key）
        group.MapPost("/api-key", async (IMediator mediator, CancellationToken ct) =>
                await mediator.Send(new GenerateApiKeyCommand(), ct))
            .RequireAuthorization()
            .Produces<ApiResult<GenerateApiKeyResponse>>(StatusCodes.Status200OK)
            .WithName("UserGenerateApiKey")
            .WithDescription("生成/刷新当前用户的长期 API Key");

        // 查看 API Key（需鉴权，校验登录密码后返回已存在的 Key）
        group.MapPost("/api-key/view", async (ViewApiKeyCommand cmd, IMediator mediator, CancellationToken ct) =>
                await mediator.Send(cmd, ct))
            .RequireAuthorization()
            .Produces<ApiResult<GenerateApiKeyResponse>>(StatusCodes.Status200OK)
            .WithName("UserViewApiKey")
            .WithDescription("查看当前用户的 API Key（需登录密码校验）");

        // 余额查询
        group.MapGet("/balance", async (IMediator mediator, CancellationToken ct) =>
                await mediator.Send(new GetUserBalanceQuery(), ct))
            .RequireAuthorization()
            .Produces<ApiResult<UserBalanceResponse>>(StatusCodes.Status200OK)
            .WithName("UserBalance")
            .WithDescription("查询当前用户余额");
    }
}
