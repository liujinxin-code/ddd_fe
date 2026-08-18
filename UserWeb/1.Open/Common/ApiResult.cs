using System;
using System.Collections;
using System.Collections.Generic;

namespace Open.Common.Models
{
    /// <summary>
    /// HTTP 传输信封（仅存在于 HTTP 层 1.Open）。应用层不应再引用本类型，
    /// 业务结果由 Controller / 中间件在边缘统一包装。
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// 200成功 400参数异常 401登录校验失败  500内部错误
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 返回消息内容
        /// </summary>
        public string Message { get; set; } = default!;

        public static ApiResult Successed(string message = "Success!") => new ApiResult
        {
            Code = 200,
            Message = message
        };

        public static ApiResult UnAuth(string message = "登录失效!") => new ApiResult
        {
            Code = 401,
            Message = message
        };
        public static ApiResult Forbidden(string message = "没有权限!") => new ApiResult
        {
            Code = 403,
            Message = message
        };

        public static ApiResult Failed(string message)
        {
            return new ApiResult
            {
                Code = 400,
                Message = message
            };
        }

        public static ApiResult Error(string message = "系统繁忙！")
        {
            return new ApiResult
            {
                Code = 500,
                Message = message
            };
        }
    }

    public class ApiResult<T>
    {
        /// <summary>
        /// 200成功 400参数异常 401登录校验失败  500内部错误
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 返回消息内容
        /// </summary>
        public string Message { get; set; } = default!;
        /// <summary>
        /// 数据体
        /// </summary>
        public T Data { get; set; } = default!;
        /// <summary>
        /// 数据条数
        /// </summary>
        public int DataTotal { get; set; }

        /// <summary>
        /// 注意：当 data 为 IList 时，DataTotal 取 list.Count（忽略 dataCount 参数）。
        /// 分页场景请使用 Open.Common.Models 中 Controller 的 ApiPaged 辅助方法显式构造信封。
        /// </summary>
        public static ApiResult<T> Successed(T data, int dataCount = 1, string message = "Success!") => new ApiResult<T>
        {
            Data = data,
            DataTotal = data is IList list ? list.Count : dataCount,
            Code = 200,
            Message = message
        };
    }
}
