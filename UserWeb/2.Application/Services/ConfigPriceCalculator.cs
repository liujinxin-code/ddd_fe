using Domain.Entities;
using Shared.Utilities;
using System.Collections.Generic;

namespace Application.Services
{
    /// <summary>
    /// 当前用户的定价上下文，由 Handler 一次性装配后传入计算器，避免 N+1 查询。
    /// 仅用“是否存在记录”判定是否有单独定价/加价（记录存在即采用其金额，金额本身可为 0）。
    /// </summary>
    public sealed class UserPriceContext
    {
        /// <summary>当前用户是否有上级代理（true=代理的下级用户，false=网站直属用户）</summary>
        public bool HasAgent { get; init; }

        /// <summary>当前用户自己的单独定价：configId -> custom_price（仅含存在记录）</summary>
        public Dictionary<int, decimal> UserCustom { get; init; } = new();

        /// <summary>上级代理的单独定价（代理“进货价”）：configId -> custom_price</summary>
        public Dictionary<int, decimal> AgentCustom { get; init; } = new();

        /// <summary>代理对单业务的加价金额：configId -> markup_add_price</summary>
        public Dictionary<int, decimal> AgentMarkup { get; init; } = new();

        /// <summary>是否存在代理总体加价配置</summary>
        public bool HasOverall { get; init; }

        /// <summary>代理总体加价百分比（[0,200]），HasOverall 为 false 时无意义</summary>
        public int OverallPercent { get; init; }
    }

    /// <summary>
    /// 业务配置价格计算器（纯函数，无副作用）。
    /// 规则（已与用户确认，2026-08-01）：
    /// 1) 网站直属用户（无上级代理）：最终单价 = 用户单独定价 ?? 系统底价(config_price)。
    /// 2) 代理的下级用户：
    ///    代理基准价 = 代理单独定价 ?? 系统底价；
    ///    优先级：单业务加价(markup) &gt; 总体百分比(overall) &gt; 无加价(=基准价)；
    ///    - 有 markup：基准价 + markup_add_price
    ///    - 否则有 overall：基准价 × (1 + overall_percent/100)
    ///    - 否则：基准价
    /// 3) 展示价 = show_price_unit × 最终单价。
    /// </summary>
    public static class ConfigPriceCalculator
    {
        public static decimal CalculateUnitPrice(TkConfig config, UserPriceContext ctx)
        {
            // 直属用户：custom(user) ?? config_price
            if (!ctx.HasAgent)
            {
                if (ctx.UserCustom.TryGetValue(config.ConfigId, out var userCustom))
                {
                    return Utils.RoundToSixDecimals(userCustom);
                }
                return Utils.RoundToSixDecimals(config.ConfigPrice);
            }

            // 有上级代理：基准价 = 代理单独定价 ?? 系统底价
            decimal basePrice = ctx.AgentCustom.TryGetValue(config.ConfigId, out var agentCustom)
                ? agentCustom
                : config.ConfigPrice;

            // 优先级：单业务加价 > 总体百分比 > 基准价
            decimal price;
            if (ctx.AgentMarkup.TryGetValue(config.ConfigId, out var markup))
            {
                price = basePrice + markup;
            }
            else if (ctx.HasOverall)
            {
                price = basePrice * (1m + ctx.OverallPercent / 100m);
            }
            else
            {
                price = basePrice;
            }

            return Utils.RoundToSixDecimals(price);
        }
    }
}
