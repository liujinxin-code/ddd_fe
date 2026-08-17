using Domain.Enums;

namespace Application.Features.Config.Models
{
    /// <summary>
    /// 首页业务配置项。携带业务信息与“当前登录用户看到的价格”。
    /// 价格字段说明：
    /// - UnitPrice：经单独定价/代理加价后的当前用户最终单价（数量单个）。
    /// - DisplayPrice：前台展示价 = ShowPriceUnit × UnitPrice（如“1000个/50元”中的 50）。
    /// - ShowPriceUnit：展示单位（如 1000）。
    /// 注：系统底价（成本）不回传前台，避免暴露毛利。
    /// </summary>
    public class ConfigResponse
    {
        /// <summary>配置id</summary>
        public int ConfigId { get; set; }

        /// <summary>业务/配置名称</summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>配置提示</summary>
        public string ConfigNotice { get; set; } = string.Empty;

        /// <summary>当前用户最终单价（数量单个）</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>展示价格单位（如 1000），0 表示未配置展示单位</summary>
        public int ShowPriceUnit { get; set; }

        /// <summary>前台展示价 = ShowPriceUnit × UnitPrice</summary>
        public decimal DisplayPrice { get; set; }

        /// <summary>最小下单数量（0 表示无下限约束）</summary>
        public int MinQuantity { get; set; }

        /// <summary>最大下单数量（0 表示无上限约束）</summary>
        public int MaxQuantity { get; set; }

        /// <summary>订单数量必须被此单位整除（0 表示无整除约束）</summary>
        public int OrderUnit { get; set; }

        /// <summary>模板类型</summary>
        public JsonTemplate JsonTemplate { get; set; }

        /// <summary>排序</summary>
        public int ConfigSort { get; set; }
    }
}
