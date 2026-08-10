using System;
using System.Collections.Generic;
using System.Globalization;
using Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Exceptions;
using ShardingCore.VirtualRoutes.Abstractions;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// 订单按月分表路由（ShardingCore）。
    ///
    /// 路由键 = tk_order.order_no。订单号格式为 O + yyMMddHHmmssfff + 随机串（见 Utils.GenerateSerialNo("O")），
    /// 内嵌下单日期；解析第 1~6 位 yyMMdd 即可得出所属月份，物理尾表命名为 tk_order_yyyyMM。
    ///
    /// 与“按 CreateTime 路由”相比，尾表月份完全一致（订单号与创建时间在同一时刻生成），
    /// 但按 order_no 路由可在「按订单号等值查询」时精准命中单张尾表，而非广播全部尾表。
    ///
    /// 继承 AbstractShardingAutoCreateOperatorVirtualTableRoute：
    /// - CalcTailsOnStart 给出 历史起点 ~ 当月+缓冲 的全部尾表，配合 CreateShardingTableOnStart 在建表配置中创建；
    /// - AutoCreateTableByTime + 月度 cron 作为兜底，自动补偿未来月份表。
    /// 评论表 tk_comment 不在此路由内，保持单表（与订单解耦，不建跨分片外键）。
    /// </summary>
    public class OrderNoMonthVirtualTableRoute : AbstractShardingAutoCreateOperatorVirtualTableRoute<TkOrder, string>
    {
        /// <summary>
        /// 历史数据起点：应不晚于库中最早一笔订单的月份（实际项目自 2024 年起）。
        /// 若早于此日期的历史订单未迁移，需在 CalcTailsOnStart 前扩起点或先做数据迁移。
        /// </summary>
        private static readonly DateTime BeginTime = new DateTime(2026, 6, 1);

        /// <summary>
        /// 向前预建缓冲月数：保证未来一段时间内的下单尾表一定存在，不纯粹依赖 cron 行为。
        /// 配合定期发布，可覆盖任意长周期。
        /// </summary>
        private const int ForwardMonths = 2;

        public override void Configure(EntityMetadataTableBuilder<TkOrder> builder)
        {
            builder.ShardingProperty(o => o.OrderNo);
        }

        /// <summary>
        /// 将分表键值（OrderNo）转换为物理尾表后缀 yyyyMM。
        /// </summary>
        public override string ShardingKeyToTail(object shardingKey)
        {
            var orderNo = shardingKey?.ToString() ?? string.Empty;
            if (orderNo.Length < 7 || orderNo[0] != 'O')
            {
                throw new ShardingCoreException($"无法从 OrderNo 解析分表月份: {orderNo}");
            }

            // 第 1~6 位为 yyMMdd
            var datePart = orderNo.Substring(1, 6);
            if (!DateTime.TryParseExact(datePart, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate))
            {
                throw new ShardingCoreException($"OrderNo 日期片段无法解析: {datePart}");
            }

            return orderDate.ToString("yyyyMM");
        }

        /// <summary>
        /// 查询路由：OrderNo 等值条件时精准命中该月尾表；其余（如按 userid 查我的订单）广播全部尾表。
        /// </summary>
        public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal:
                    var tail = ShardingKeyToTail(shardingKey);
                    return t => t == tail;
                default:
                    return t => true;
            }
        }

        /// <summary>
        /// 启动时的已知尾表集合：历史起点 ~ 当月+缓冲。CreateShardingTableOnStart 会据此建表。
        /// </summary>
        protected override List<string> CalcTailsOnStart()
        {
            var tails = new List<string>();
            var cursor = new DateTime(BeginTime.Year, BeginTime.Month, 1);
            var limit = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(ForwardMonths);

            while (cursor <= limit)
            {
                tails.Add(cursor.ToString("yyyyMM"));
                cursor = cursor.AddMonths(1);
            }

            return tails;
        }

        public override bool AutoCreateTableByTime() => true;

        /// <summary>每月 1 日 00:00 触发一次尾表补偿（建下月表）。</summary>
        public override string[] GetCronExpressions() => new[] { "0 0 0 1 * ?" };

        /// <summary>将“当前时间”映射到尾表后缀（供自动建表任务使用）。</summary>
        protected override string ConvertNowToTail(DateTime now) => now.ToString("yyyyMM");
    }
}
