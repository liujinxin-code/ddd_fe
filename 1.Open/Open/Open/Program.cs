using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;
using Open.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseInfrastructureSerilog(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();

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
    //  全局要求 JWT
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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

var app = builder.Build();

// 全新部署：若目标库尚无表，按 EF 模型自动建表（仓库当前无 EF 迁移文件）。
// 仅适合首次部署；后续表结构演进请改用 EF Migrations（dotnet ef migrations add）。
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.AppDbContext>();
    db.Database.EnsureCreated();
}

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

app.UseAuthentication();
app.UseAuthorization();

// 滑动窗口限流：授权接口按 userid:jti 限流，匿名接口按客户端 IP 限流
app.UseMiddleware<RateLimitMiddleware>();

// 启用静态文件服务（工单图片上传到 wwwroot/images/... 后通过 /images/... 访问）
app.UseStaticFiles();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
