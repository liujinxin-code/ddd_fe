using Application.Common.Models;
using FluentValidation;
using Shared.Exceptions;


namespace Open.Middleware
{
    /// <summary>
    /// 异常处理中间件
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var result = ex switch
            {
                InvalidOperationException or
                ValidationException or
                BusinessException => ApiResult.Failed(ex.Message),
                UnauthorizedAccessException => ApiResult.UnAuth("未登录或登录已失效"),
                ConcurrencyConflictException => ApiResult.Error(ex.Message),
                _ => ApiResult.Error("服务器内部错误")
            };

            switch (ex)
            {
                case ConcurrencyConflictException:
                    _logger.LogError(ex,
                    "需要查看的异常 | TraceId={0},Message={1}",
                    context.TraceIdentifier, ex.Message);
                    break;
                default:
                    _logger.LogError(ex,
                "未处理异常 | TraceId={0},Message={1}",
                context.TraceIdentifier, ex.Message);
                    break;
            }

            context.Response.StatusCode = result.Code;
            return context.Response.WriteAsJsonAsync(result);
        }
    }


}
