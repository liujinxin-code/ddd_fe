using Application.Abstractions;
using Application.Common.Models;
using Open.Common.Models;
using Infrastructure.Common.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Open.Controllers;
using System.IO;

namespace Open.Controllers;

/// <summary>
/// 通用文件上传。
/// 设计约定（见需求）：
///  - 不限制文件类型，类型约束交给前端（前端 accept / beforeUpload 控制）；
///  - 限制单文件大小 5MB（硬上限），以及单次上传数量（通过 IOptions&lt;FileOptions&gt; 读取，配置节 FileSettings:MaxFileCount，默认 10）；
///  - 上传目录逻辑与历史一致：wwwroot/images/yyyyMMdd/{GUID}{原始扩展名}；
///  - 返回可直接访问的完整 http(s) URL，前缀来自 IOptions&lt;FileOptions&gt; 的 BaseUrl（配置节 FileSettings:BaseUrl）。
/// 全部 [Authorize]，仅登录用户可上传。
/// </summary>
[Route("api/[controller]/")]
[ApiController]
[Authorize]
public class FileController(IWebHostEnvironment env, IOptions<FileUploadOptions> fileOptions, ICurrentUser currentUser) : BaseController
{
    // 单文件大小硬上限 5MB（需求明确，保持常量，不对外暴露以防误调大）
    private const long MaxFileBytes = 5 * 1024 * 1024;

    private const string UploadFolder = "images";

    private readonly FileUploadOptions _fileOptions = fileOptions.Value;

    // 单次上传数量上限：可配置，缺省 10（安全网，防止一次性传大量文件打满请求/磁盘）
    private int MaxFileCount => _fileOptions.MaxFileCount;

    // 文件访问基址：必须在 appsettings 的 FileSettings:BaseUrl 配置
    private string BaseUrl => _fileOptions.BaseUrl.TrimEnd('/');

    /// <summary>
    /// 通用文件上传：接收多个文件，保存到 wwwroot/images/yyyyMMdd/，返回完整可访问 URL 列表。
    /// 仅限制单文件≤5MB 与数量上限，不限制文件类型。
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 安全网：防止超大请求体先被框架拒绝；精确上限在方法内按 MaxFileCount 校验
    public async Task<ApiResult<List<string>>> Upload(List<IFormFile> files, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            return new ApiResult<List<string>> { Code = 401, Message = "登录失效", Data = new(), DataTotal = 0 };

        if (files == null || files.Count == 0)
            return new ApiResult<List<string>> { Code = 400, Message = "请选择要上传的文件" };
        if (files.Count > MaxFileCount)
            return new ApiResult<List<string>> { Code = 400, Message = $"单次最多上传 {MaxFileCount} 个文件" };

        var dateDir = DateTime.Now.ToString("yyyyMMdd");
        var targetDir = Path.Combine(env.WebRootPath, UploadFolder, dateDir);
        Directory.CreateDirectory(targetDir);

        var urls = new List<string>(files.Count);
        foreach (var file in files)
        {
            if (file == null || file.Length == 0)
                return new ApiResult<List<string>> { Code = 400, Message = "存在空文件，请重新选择" };
            if (file.Length > MaxFileBytes)
                return new ApiResult<List<string>> { Code = 400, Message = $"文件「{file.FileName}」超过 5MB 限制" };

            // 不限制类型：保留原始扩展名（前端已按业务限制为图片）
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(targetDir, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            urls.Add($"{BaseUrl}/{UploadFolder}/{dateDir}/{fileName}");
        }

        return new ApiResult<List<string>> { Code = 200, Message = "上传成功", Data = urls, DataTotal = urls.Count };
    }
}
