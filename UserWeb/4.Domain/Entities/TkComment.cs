using System;
using Domain.Auditors;

namespace Domain.Entities
{
    /// <summary>
    /// 订单评论，对应 tk_comment 表。
    /// 作为「订单」聚合下的子实体：评论模板(json_template=2)的业务在下单时一并提交评论内容，
    /// 一条评论 = 一个下单数量。
    /// 评论通过 order_no（业务订单号）逻辑关联订单（数据库列已从 order_id 改为 order_no）：
    /// 因订单按月分表（ShardingCore），跨分片外键/级联不安全，故不建立 EF 导航关系，
    /// 下单时由领域方法显式写入 OrderNo 后随订单一起落库。
    /// 软删除走 DeleteAuditor 统一规范（is_delete + delete_time），预留「后期更换评论」的场景。
    /// 字段遵循项目约定：除 delete_time 外一律非空。
    /// </summary>
    public class TkComment : DeleteAuditor
    {
        /// <summary>评论自增主键</summary>
        public int CommentId { get; private set; }

        /// <summary>关联订单号（tk_comment.order_no = tk_order.order_no，业务订单号，非空）</summary>
        public string OrderNo { get; private set; } = default!;

        /// <summary>评论内容（最长 500 字符）</summary>
        public string CommentContent { get; private set; } = default!;

        /// <summary>评论状态，暂时固定为 1（预留）</summary>
        public int CommentState { get; private set; }

        /// <summary>发表评论的用户id</summary>
        public long Userid { get; private set; }

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkComment() { }

        /// <summary>
        /// 随订单创建一条评论。OrderNo 由调用方（订单聚合根）显式传入，
        /// 由于订单按月分表、评论单表，评论与订单不建立跨分片 EF 外键，order_no 仅作逻辑关联。
        /// </summary>
        public TkComment(string commentContent, long userId, string orderNo)
        {
            if (string.IsNullOrWhiteSpace(commentContent))
                throw new ArgumentException("评论内容不能为空", nameof(commentContent));
            if (string.IsNullOrWhiteSpace(orderNo))
                throw new ArgumentException("订单号不能为空", nameof(orderNo));

            var content = commentContent.Trim();
            if (content.Length > 500)
                throw new ArgumentException("评论内容不能超过 500 字符", nameof(commentContent));

            CommentContent = content;
            Userid = userId;
            OrderNo = orderNo;
            CommentState = 1;
        }

        // 假删除（后期更换评论时把旧评论标记为已删除而非物理删除）复用基类 MarkDeletedFunc()。
    }
}
