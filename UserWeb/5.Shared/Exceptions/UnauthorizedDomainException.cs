using System;

namespace Shared.Exceptions
{
    /// <summary>
    /// 鉴权/授权失败的领域异常。在应用层 Handler 中替代原先直接构造 ApiResult(401) 的写法，
    /// 由 Open 层 ExceptionHandlingMiddleware 统一映射为 ApiResult(401)。
    /// </summary>
    public class UnauthorizedDomainException : Exception
    {
        public UnauthorizedDomainException(string message = "登录失效或未授权") : base(message)
        {
        }
    }
}
