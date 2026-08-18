using System.Collections.Generic;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Open.Common.Models;

namespace Open.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// 将应用层纯业务结果包装为 HTTP 信封（单对象 / 列表 / 标量）。
        /// </summary>
        protected IActionResult Api<T>(T data) => Ok(ApiResult<T>.Successed(data));

        /// <summary>
        /// 将分页载体 PagedResult 包装为 HTTP 信封，正确回填 Data(Items) 与 DataTotal(TotalCount)。
        /// 注意：不能直接用 ApiResult&lt;T&gt;.Successed(list, total) —— 因 data 为 IList 时 DataTotal 会被忽略，
        /// 故此处显式构造信封。
        /// </summary>
        protected IActionResult ApiPaged<T>(PagedResult<T> p) =>
            Ok(new ApiResult<List<T>>
            {
                Code = 200,
                Message = "Success!",
                Data = p.Items.ToList(),
                DataTotal = p.TotalCount
            });

        /// <summary>
        /// 无返回数据的成功信封（对应应用层 Unit）。
        /// </summary>
        protected IActionResult Api() => Ok(ApiResult.Successed());
    }
}
