using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;
using Open.Filters;
using Open.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseInfrastructureSerilog(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers(o => o.Filters.Add<CurrentUserFilter>());

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
