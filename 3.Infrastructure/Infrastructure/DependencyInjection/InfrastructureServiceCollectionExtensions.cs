using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Abstractions.Caching;
using Application.Abstractions.Passwords;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Events.Agent.Contracts;
using Infrastructure.Common.Auth;
using Infrastructure.Common.Caching;
using Infrastructure.Common.Passwords;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            //数据库初始化
            var dbOptions = configuration.GetSection(DbOptions.SectionName).Get<DbOptions>();
            services.AddDbContext<AppDbContext>(options => options.UseMySql(dbOptions!.ConnectionString,
    new MySqlServerVersion(new Version(8, 0, 0))));

            //Redis注入并且 redis工具类泛型注入（单例）
            var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();
            var multiplexer = ConnectionMultiplexer.Connect(redisOptions!.ConnectionString);
            services.AddSingleton(multiplexer);
            services.AddSingleton(typeof(ICacheService<>), typeof(CacheService<>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITkUserRepository, TkUserRepository>();
            services.AddScoped<IConsumeLogRepository, ConsumeLogRepository>();
            services.AddScoped<IPlatformRepository, PlatformRepository>();
            services.AddScoped<IConfigRepository, ConfigRepository>();
            services.AddScoped<IAgentPricingRepository, AgentPricingRepository>();
            services.AddScoped<INoticeRepository, NoticeRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();

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
