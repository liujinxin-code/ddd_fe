using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Abstractions.Caching;
using Application.Abstractions.Passwords;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Features.Agent;
using Infrastructure.Common.Auth;
using Infrastructure.Common.Caching;
using Infrastructure.Common.Passwords;
using Infrastructure.Common;
using Infrastructure.Common.Files;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Common.RateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShardingCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<PasswordOptions>().Bind(configuration.GetSection(PasswordOptions.SectionName))
          .Validate(x => !string.IsNullOrWhiteSpace(x.Salt), "PasswordOptions:Salt 不能为空").ValidateOnStart();

            services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName))
      .Validate(x => !string.IsNullOrWhiteSpace(x.Issuer), "JwtSettings:Issuer 不能为空").ValidateOnStart()
        .Validate(x => !string.IsNullOrWhiteSpace(x.Audience), "JwtSettings:Audience 不能为空").ValidateOnStart()
          .Validate(x => !string.IsNullOrWhiteSpace(x.SecretKey), "JwtSettings:SecretKey 不能为空").ValidateOnStart();

            services.AddOptions<RedisOptions>().Bind(configuration.GetSection(RedisOptions.SectionName))
    .Validate(x => !string.IsNullOrWhiteSpace(x.ConnectionString), "RedisOptions:ConnectionString 不能为空").ValidateOnStart();

            services.AddOptions<DbOptions>().Bind(configuration.GetSection(DbOptions.SectionName))
.Validate(x => !string.IsNullOrWhiteSpace(x.ConnectionString), "DbOptions:ConnectionString 不能为空").ValidateOnStart();

            services.AddOptions<FileUploadOptions>().Bind(configuration.GetSection(FileUploadOptions.SectionName))
                .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "FileSettings:BaseUrl 不能为空").ValidateOnStart()
                .Validate(x => x.MaxFileCount > 0, "FileSettings:MaxFileCount 必须大于 0").ValidateOnStart();

            //数据库初始化（ShardingCore 分表）
            // - AddShardingDbContext 替代原生 AddDbContext；订单按 order_no 月份路由到 tk_order_yyyyMM。
            // - UseConfig 中 UseShardingQuery/UseShardingTransaction 配置物理库连接方式；
            //   AddDefaultDataSource 指定默认数据源；EnsureCreatedWithOutShardingTable 负责非分表实体建表；
            //   CreateShardingTableOnStart 依据路由 CalcTailsOnStart 在建表时创建已知尾表。
            var dbOptions = configuration.GetSection(DbOptions.SectionName).Get<DbOptions>();
            services.AddShardingDbContext<AppDbContext>()
                .UseRouteConfig(op => op.AddShardingTableRoute<OrderNoMonthVirtualTableRoute>())
                .UseConfig(op =>
                {
                    op.UseShardingQuery((conn, builder) => builder.UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 0))));
                    op.UseShardingTransaction((conn, builder) => builder.UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 0))));
                    op.AddDefaultDataSource("ds0", dbOptions!.ConnectionString);
                })
                .AddShardingCore();

            //Redis注入并且 redis工具类泛型注入（单例）
            var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();
            var multiplexer = ConnectionMultiplexer.Connect(redisOptions!.ConnectionString);
            services.AddSingleton(multiplexer);
            services.AddSingleton(typeof(ICacheService<>), typeof(CacheService<>));

            // 滑动窗口限流：配置 + 限流器（依赖上面的 ConnectionMultiplexer 单例）
            services.AddOptions<RateLimitOptions>()
                .Bind(configuration.GetSection(RateLimitOptions.SectionName));
            services.AddSingleton<ISlidingWindowRateLimiter, SlidingWindowRateLimiter>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITkUserRepository, TkUserRepository>();
            services.AddScoped<IConsumeLogRepository, ConsumeLogRepository>();
            services.AddScoped<IPlatformRepository, PlatformRepository>();
            services.AddScoped<IConfigRepository, ConfigRepository>();
            services.AddScoped<IAgentPricingRepository, AgentPricingRepository>();
            services.AddScoped<INoticeRepository, NoticeRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<IServiceImageRepository, ServiceImageRepository>();

            services.AddScoped<IPasswordHelper, PasswordHelper>();
            services.AddScoped<ITokenCacheService, TokenCacheService>();
            services.AddScoped<IJwtHelper, JwtHelper>();

            // 当前登录用户：基于已通过 [Authorize] 验证的 JWT Claims，供 Application 层 Handler 注入。
            // 用户身份只源于服务端 JWT，前台无法伪造。
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUserAccessor>();

            return services;

        }
    }
}
