using Application.Common.Models;
using Application.Common.Models.Enum;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Reflection;

namespace Open.Controllers
{
    /// <summary>
    /// 枚举同步：向前端暴露常用业务枚举的 value/name/label，避免前后端硬编码不一致。
    /// </summary>
    [Authorize]
    public class EnumController : BaseController
    {
        /// <summary>消费流水类型（ConsumeStatus）</summary>
        [HttpPost("consume-status")]
        public ApiResult<List<EnumOption>> ConsumeStatus()
            => Ok(Convert<ConsumeStatus>());

        /// <summary>订单状态（OrderState）</summary>
        [HttpPost("order-state")]
        public ApiResult<List<EnumOption>> OrderState()
            => Ok(Convert<OrderState>());

        /// <summary>工单状态（TicketStatus）</summary>
        [HttpPost("ticket-status")]
        public ApiResult<List<EnumOption>> TicketStatus()
            => Ok(Convert<TicketStatus>());

        /// <summary>工单问题类型（TicketType）</summary>
        [HttpPost("ticket-type")]
        public ApiResult<List<EnumOption>> TicketType()
            => Ok(Convert<TicketType>());

        private static ApiResult<List<EnumOption>> Ok(List<EnumOption> data)
            => new() { Code = 200, Message = "Success!", Data = data, DataTotal = data.Count };

        private static List<EnumOption> Convert<TEnum>() where TEnum : struct, Enum
        {
            var type = typeof(TEnum);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            var list = new List<EnumOption>(fields.Length);

            foreach (var field in fields)
            {
                var value = (int)field.GetValue(null)!;
                var desc = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name;
                list.Add(new EnumOption
                {
                    Value = value,
                    Name = field.Name,
                    Label = desc,
                });
            }

            return list.OrderBy(x => x.Value).ToList();
        }
    }
}
