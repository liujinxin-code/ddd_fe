using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
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

        public static ApiResult<T> Successed(T data, int dataCount = 1, string message = "Success!") => new ApiResult<T>
        {
            Data = data,
            DataTotal = data is IList list ? list.Count : dataCount,
            Code = 200,
            Message = message
        };
    }
}
