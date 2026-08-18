using System.Collections.Generic;

namespace Application.Common.Models
{
    /// <summary>
    /// 中性分页载体（应用层内部使用，不含任何 HTTP/传输概念）。
    /// 由分页类 Handler 返回，HTTP 层（Controller）据此构造 ApiResult 信封的 Data/DataTotal。
    /// </summary>
    public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
}
