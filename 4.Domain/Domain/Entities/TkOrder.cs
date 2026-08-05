using System;
using System.Collections.Generic;
using Domain.Auditors;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// 订单，对应 tk_order 表。
    /// 下单时由下单 Handler 解析出「用户单价/订单金额/代理利润」后通过构造函数物化；
    /// 履约相关字段（成功数量/结束数量/推送状态/第三方单号）初始为 0/空，由后续履约流程回填。
    /// 金额精度：order_amount 为 decimal(11,6)，代理相关金额 decimal(10,6)。
    /// 约定：int/string 字段强制非空（与数据库 NOT NULL 对齐）；代理相关字段在「无上级代理」时记为 0。
    /// </summary>
    public class TkOrder : CreateAuditor
    {
        /// <summary>订单自增主键</summary>
        public int OrderId { get; private set; }

        /// <summary>订单序号（业务订单号，唯一）</summary>
        public string OrderNo { get; private set; } = default!;

        /// <summary>订单状态 1 正在执行 / 2 已完单 / 3 部分完成 / 4 已取消（未收费）</summary>
        public OrderState OrderState { get; private set; }

        /// <summary>下单链接（增量业务必填；账户业务选填，留空串）</summary>
        public string OrderLink { get; private set; } = default!;

        /// <summary>业务配置id（tk_config.config_id）</summary>
        public int ConfigId { get; private set; }

        /// <summary>下单用户id</summary>
        public int Userid { get; private set; }

        /// <summary>订单金额（用户最终单价 × 数量，decimal(11,6)）</summary>
        public decimal OrderAmount { get; private set; }

        /// <summary>订单数量（增量业务=链接数量；账户业务=购买账户个数）</summary>
        public int Quantity { get; private set; }

        /// <summary>成功数量（履约更新，初始 0）</summary>
        public int SuccessQuantity { get; private set; }

        /// <summary>初始数量（= 下单数量）</summary>
        public int BeginQuantity { get; private set; }

        /// <summary>结束数量（履约更新，初始 0）</summary>
        public int EndQuantity { get; private set; }

        /// <summary>推送状态 0 未推送 / 1 已推送 / 9 推送异常（初始 0）</summary>
        public int PushState { get; private set; }

        /// <summary>第三方订单号（履约回填，初始空）</summary>
        public string SerialNo { get; private set; } = default!;

        /// <summary>渠道id（取自业务配置）</summary>
        public int ChannelId { get; private set; }

        /// <summary>渠道服务id（取自业务配置）</summary>
        public int ChannelServerId { get; private set; }

        /// <summary>代理id（下单用户的上级代理；无上级为 0）</summary>
        public int AgentUserid { get; private set; }

        /// <summary>代理单个加价/单位利润（decimal(10,6)，无代理为 0）</summary>
        public decimal AgentSingleAddPrice { get; private set; }

        /// <summary>是否已计算代理差价 0 否 / 1 是（初始 0，结算时置 1）</summary>
        public int IsDifference { get; private set; }

        /// <summary>代理此订单总赚取金额（decimal(10,6)，无代理为 0）</summary>
        public decimal AgentOrderAmount { get; private set; }

        /// <summary>
        /// 退费金额（decimal(11,6)，默认 0）。
        /// 订单部分完成或取消时由履约/结算流程回填，下单时恒为 0。
        /// </summary>
        public decimal RefundAmount { get; private set; }

        private readonly List<TkComment> _comments = new();

        /// <summary>
        /// 订单附带的评论内容（仅评论模板业务有值）。
        /// 作为聚合子实体随订单一起落库，EF 在同一次 SaveChanges 内回填 tk_comment.order_id。
        /// </summary>
        public IReadOnlyCollection<TkComment> Comments => _comments;

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkOrder() { }

        /// <summary>
        /// 物化一条新建订单。价格与代理利润已由调用方（下单 Handler）计算完成。
        /// </summary>
        public TkOrder(
            string orderNo,
            int configId,
            long userId,
            string orderLink,
            decimal orderAmount,
            int quantity,
            int channelId,
            int channelServerId,
            long agentUserId,
            decimal agentSingleAddPrice,
            decimal agentOrderAmount)
        {
            if (string.IsNullOrWhiteSpace(orderNo)) throw new ArgumentException("订单号不能为空", nameof(orderNo));
            if (configId <= 0) throw new ArgumentException("业务配置id无效", nameof(configId));
            if (userId <= 0) throw new ArgumentException("用户id无效", nameof(userId));
            if (orderAmount <= 0) throw new ArgumentException("订单金额必须大于 0", nameof(orderAmount));
            if (quantity <= 0) throw new ArgumentException("订单数量必须大于 0", nameof(quantity));

            OrderNo = orderNo;
            ConfigId = configId;
            Userid = (int)userId;
            OrderLink = orderLink ?? string.Empty;
            OrderAmount = orderAmount;
            Quantity = quantity;
            BeginQuantity = quantity;
            SuccessQuantity = 0;
            EndQuantity = 0;
            PushState = 0;
            SerialNo = string.Empty;
            ChannelId = channelId;
            ChannelServerId = channelServerId;
            AgentUserid = (int)agentUserId;
            AgentSingleAddPrice = agentSingleAddPrice;
            IsDifference = 0;
            AgentOrderAmount = agentOrderAmount;
            RefundAmount = 0m;
            OrderState = Domain.Enums.OrderState.Running;
        }

        /// <summary>
        /// 为订单追加一条评论（评论模板业务专用）。一条评论对应一个下单数量。
        /// </summary>
        public void AddCommentFunc(string commentContent, long userId)
        {
            _comments.Add(new TkComment(commentContent, userId));
        }
    }
}
