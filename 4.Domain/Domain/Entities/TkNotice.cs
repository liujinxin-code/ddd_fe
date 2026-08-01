using System;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// 公告，对应 tk_notice 表。
    /// 前台只读模型：公告的新增 / 修改由后台管理，前台仅查询展示（首页置顶+普通公告、弹窗公告）。
    /// 字段遵循领域约定强制非空：string 给 default!，int / 枚举 / DateTime 非空，实体定义无 ?。
    /// </summary>
    public class TkNotice
    {
        /// <summary>公告id（主键，后台赋值，非自增）</summary>
        public int NoticeId { get; private set; }

        /// <summary>公告内容（varchar(2000)）</summary>
        public string NoticeContent { get; private set; } = default!;

        /// <summary>公告类型（1 置顶 / 2 普通 / 3 弹窗）</summary>
        public NoticeType NoticeType { get; private set; }

        /// <summary>创建时间（datetime）</summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkNotice() { }
    }
}
