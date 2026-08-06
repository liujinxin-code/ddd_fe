using Application.Abstractions;
using Application.Common.Models;
using Application.Common.Models.Ticket;
using Application.Events.Ticket.Contracts.Commands;
using Application.Events.Ticket.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Open.Controllers;
using System.IO;
using System.Security.Claims;

namespace Open.Controllers;

/// <summary>
/// 客服工单：图片上传、提交工单、我的工单列表。
/// 全部 [Authorize]，用户身份由 JWT 注入，列表/提交仅限本人。
/// </summary>
[Route("api/[controller]/")]
[ApiController]
[Authorize]
public class TicketController(IMediator mediator, IWebHostEnvironment env, ICurrentUser currentUser) : BaseController
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg" };

    private const long MaxFileBytes = 5 * 1024 * 1024; // 5MB
    private const int MaxFileCount = 5;

    /// <summary>
    /// 上传工单图片：单个≤5MB、最多5张、仅允许 png/jpg，
    /// 保存到 wwwroot/images/yyyyMMdd/{GUID}.xxx，返回可公开访问的相对 URL 列表。
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileBytes * (MaxFileCount + 1))] // 5 张上限 + 余量，防止超大请求
    public async Task<ApiResult<List<string>>> UploadImages(List<IFormFile> files, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            return new ApiResult<List<string>> { Code = 401, Message = "登录失效", Data = new(), DataTotal = 0 };

        if (files == null || files.Count == 0)
            return new ApiResult<List<string>> { Code = 400, Message = "请选择要上传的图片" };
        if (files.Count > MaxFileCount)
            return new ApiResult<List<string>> { Code = 400, Message = $"最多上传 {MaxFileCount} 张图片" };

        var dateDir = DateTime.UtcNow.ToString("yyyyMMdd");
        var targetDir = Path.Combine(env.WebRootPath, "images", dateDir);
        Directory.CreateDirectory(targetDir);

        var urls = new List<string>(files.Count);
        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
                return new ApiResult<List<string>> { Code = 400, Message = "存在空文件，请重新选择" };
            if (file.Length > MaxFileBytes)
                return new ApiResult<List<string>> { Code = 400, Message = $"图片「{file.FileName}」超过 5MB 限制" };

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
                return new ApiResult<List<string>> { Code = 400, Message = "仅支持 PNG / JPG 格式图片" };

            var fileName = $"{Guid.NewGuid()}{ext.ToLowerInvariant()}";
            var fullPath = Path.Combine(targetDir, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream, ct);
            urls.Add($"/images/{dateDir}/{fileName}");
        }

        return new ApiResult<List<string>> { Code = 200, Message = "上传成功", Data = urls, DataTotal = urls.Count };
    }

    /// <summary>提交工单（当前登录用户本人）。</summary>
    [HttpPost("create")]
    public async Task<ApiResult<long>> CreateAsync([FromBody] CreateTicketCommand cmd, CancellationToken ct)
        => await mediator.Send(cmd, ct);

    /// <summary>我的工单列表（仅当前登录用户本人）。</summary>
    [HttpPost("list")]
    public async Task<ApiResult<List<TicketListItem>>> ListAsync([FromBody] GetTicketsQuery query, CancellationToken ct)
        => await mediator.Send(query, ct);
}
