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
                _logger.LogError(ex,
                    "未处理异常 | TraceId={0}",
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var result = ex switch
            {
                ValidationException => ApiResult.Failed(ex.Message),
                BusinessException => ApiResult.Failed(ex.Message),
                UnauthorizedAccessException => ApiResult.UnAuth("未登录或登录已失效"),
                _ => ApiResult.Failed("服务器内部错误")
            };
            context.Response.StatusCode = result.Code;
            return context.Response.WriteAsJsonAsync(result);
        }
    }


}
