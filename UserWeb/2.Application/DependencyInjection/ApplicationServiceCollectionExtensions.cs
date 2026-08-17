using Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var asm = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(asm);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));  // 全局校验管道
            });

            services.AddValidatorsFromAssembly(asm);  // 注册 FluentValidation 验证器

            return services;
        }
    }
}
