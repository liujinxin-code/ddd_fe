using Domain.Enums;

namespace Application.Common.Models.Response.Config
{
    /// <summary>
    /// API 文档业务配置精简项。仅返回调用订单接口所需的关键字段，避免暴露前台展示价、排序等无关信息。
    /// </summary>
    public class ConfigApiResponse
    {
        /// <summary>配置 id</summary>
        public int ConfigId { get; set; }

        /// <summary>业务/配置名称</summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>当前用户最终单价（数量单个）</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>最小下单数量（0 表示无下限约束）</summary>
        public int MinQuantity { get; set; }

        /// <summary>最大下单数量（0 表示无上限约束）</summary>
        public int MaxQuantity { get; set; }

        /// <summary>订单数量必须被此单位整除（0 表示无整除约束）</summary>
        public int OrderUnit { get; set; }

        /// <summary>模板类型：1 粉丝 / 2 评论 / 3 购买账户</summary>
        public JsonTemplate JsonTemplate { get; set; }
    }
}
