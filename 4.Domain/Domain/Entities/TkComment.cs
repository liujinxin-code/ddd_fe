using System;
using Domain.Auditors;

namespace Domain.Entities
{
    /// <summary>
    /// 订单评论，对应 tk_comment 表。
    /// 作为「订单」聚合下的子实体：评论模板(json_template=2)的业务在下单时一并提交评论内容，
    /// 一条评论 = 一个下单数量，order_id 由 EF 在同一次 SaveChanges 内自动回填。
    /// 软删除走 DeleteAuditor 统一规范（is_delete + delete_time），预留「后期更换评论」的场景。
    /// 字段遵循项目约定：除 delete_time 外一律非空。
    /// </summary>
    public class TkComment : DeleteAuditor
    {
        /// <summary>评论自增主键</summary>
        public int CommentId { get; private set; }

        /// <summary>关联订单id（tk_order.order_id，随单创建时由 EF 回填，非空外键）</summary>
        public int OrderId { get; private set; }

        /// <summary>评论内容（最长 500 字符）</summary>
        public string CommentContent { get; private set; } = default!;

        /// <summary>评论状态，暂时固定为 1（预留）</summary>
        public int CommentState { get; private set; }

        /// <summary>发表评论的用户id</summary>
        public int Userid { get; private set; }

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkComment() { }

        /// <summary>
        /// 随订单创建一条评论。order_id 交由 EF 通过订单导航属性回填，不在此处赋值。
        /// </summary>
        public TkComment(string commentContent, long userId)
        {
            if (string.IsNullOrWhiteSpace(commentContent))
                throw new ArgumentException("评论内容不能为空", nameof(commentContent));

            var content = commentContent.Trim();
            if (content.Length > 500)
                throw new ArgumentException("评论内容不能超过 500 字符", nameof(commentContent));

            CommentContent = content;
            Userid = (int)userId;
            CommentState = 1;
        }

        // 假删除（后期更换评论时把旧评论标记为已删除而非物理删除）复用基类 MarkDeletedFunc()。
    }
}
