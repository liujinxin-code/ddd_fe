using Application.Common.Behaviors;
using Application.User;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
