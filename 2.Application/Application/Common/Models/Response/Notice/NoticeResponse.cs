using Domain.Enums;
using System;

namespace Application.Common.Models.Response.Notice
{
    /// <summary>
    /// 公告展示项（前台）。
    /// </summary>
    public class NoticeResponse
    {
        /// <summary>公告id</summary>
        public int NoticeId { get; set; }

        /// <summary>公告内容</summary>
        public string NoticeContent { get; set; } = string.Empty;

        /// <summary>公告类型（1 置顶 / 2 普通 / 3 弹窗）</summary>
        public NoticeType NoticeType { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; }
    }
}
