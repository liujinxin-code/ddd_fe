using Open.Common.Models;
using Application.Features.Ticket.Models;
using Application.Features.Ticket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Open.Controllers;

namespace Open.Controllers;

/// <summary>
/// 客服工单：提交工单、我的工单列表。
/// 图片上传统一走通用 File 控制器（/File/upload），本控制器不再保存文件。
/// 全部 [Authorize]，用户身份由 JWT 注入，列表/提交仅限本人。
/// </summary>
[Route("api/[controller]/")]
[ApiController]
[Authorize]
public class TicketController(IMediator mediator) : BaseController
{
    /// <summary>提交工单（当前登录用户本人）。</summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTicketCommand cmd, CancellationToken ct)
     {

        var r = await mediator.Send(cmd, ct);

        return Api(r);

     }

    /// <summary>我的工单列表（仅当前登录用户本人）。</summary>
    [HttpPost("list")]
    public async Task<IActionResult> ListAsync([FromBody] GetTicketsQuery query, CancellationToken ct)
     {

        var r = await mediator.Send(query, ct);

        return ApiPaged(r);

     }
}
