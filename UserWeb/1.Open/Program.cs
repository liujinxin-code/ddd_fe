using Application.DependencyInjection;
using Open.Endpoints;
using Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Open.Common;
using Open.Middlewares;
using ShardingCore;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseInfrastructureSerilog(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        // MVC / Controller 路径：全局 DateTimeOffset 按请求时区序列化（数据库统一存上海时区，转换器把上海时间换算成请求时区）。
        opt.JsonSerializerOptions.Converters.Add(new TimeZoneAwareDateTimeOffsetConverter());
        opt.JsonSerializerOptions.Converters.Add(new TimeZoneAwareNullableDateTimeOffsetConverter());
    });

// Minimal API（UserEndpoints）走独立的 JSON 配置，必须单独注册，否则这部分接口不按请求时区转换。
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new TimeZoneAwareDateTimeOffsetConverter());
    options.SerializerOptions.Converters.Add(new TimeZoneAwareNullableDateTimeOffsetConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Api文档",
        Version = "v1"
    });
    //  JWT 安全定义
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme.\r\n" +
            "Enter **Bearer** [space] and then your token.\r\n" +
            "Example: `Bearer eyJhbGciOi...`",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

var config = builder.Configuration;

builder.Services.AddApplication(config);
builder.Services.AddInfrastructure(config);

builder.Services.AddJwtAuthentication(config);

// 允许所有跨域请求（开发/联调用）。生产环境应收紧为具体前端域名。
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 响应压缩：默认启用 Gzip + Brotli，浏览器按 Accept-Encoding 协商。
// 容器内为 http，EnableForHttps 仅对未来经反代终结 TLS 的场景生效。
//builder.Services.AddResponseCompression(options =>
//{
//    options.EnableForHttps = true;
//});
// 配置 Gzip 压缩核心服务
builder.Services.AddResponseCompression(options =>
{
    // 核心开关：启用压缩，且对 HTTPS 请求也生效（.NET 8 推荐显式配置）
    options.EnableForHttps = true;
    // 添加 Gzip 压缩提供器（这是开启 Gzip 的关键）
    options.Providers.Add<GzipCompressionProvider>();
    // 扩展需要压缩的 MIME 类型（.NET 8 默认仅包含基础类型，补充业务常用类型）
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",    // API 返回的 JSON 数据（核心）
        "application/xml",    // XML 格式数据
        "text/plain",         // 纯文本
        "text/css",           // CSS 样式
        "text/javascript",    // JS 脚本
        "text/html",         // HTML 页面
    });
});
// 配置 Gzip 压缩级别（推荐 Optimal，平衡性能和压缩率）
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

var app = builder.Build();

// 响应压缩中间件：尽量靠前，包裹后续所有响应（含 401/429 及 JSON API）。
app.UseResponseCompression();

// 全新部署：按 EF 模型自动建表（仓库当前无 EF 迁移文件）。
// 在 ShardingCore 下，EnsureCreated 会同时创建「非分表实体表」(tk_user/tk_comment 等)
// 与「分表实体已知尾表」(tk_order_yyyyMM，尾表集合来自路由 CalcTailsOnStart)；
// 后续新增的月份尾表由路由的 AutoCreateTableByTime + 月度 cron 在建表阶段自动补偿。
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.AppDbContext>();
    db.Database.EnsureCreated();
}

// 启动补偿：检查缺失的物理尾表并自动创建（应对路由尾表动态新增等场景）。
app.Services.UseAutoTryCompensateTable();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // HTTPS 重定向仅用于开发环境。生产环境由反向代理（nginx 等）终结 TLS，
    // 容器内只暴露 http，否则会把反代转发的 http 请求又 307 回 https，造成死循环。
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

// 请求时区：解析 X-TimeZone 头得到时区 ID 存入请求级上下文（供 DateTimeOffset 转换器把数据库统一的上海时间换算成请求时区）。
// 必须放在端点映射之前，且尽量靠前以覆盖异常响应的序列化。
app.UseMiddleware<RequestTimeZoneMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// 滑动窗口限流：授权接口按 userid:jti 限流，匿名接口按客户端 IP 限流
app.UseMiddleware<RateLimitMiddleware>();

// 启用静态文件服务（工单图片上传到 wwwroot/images/... 后通过 /images/... 访问）
app.UseStaticFiles();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapUserEndpoints();

app.MapControllers();

app.Run();
