using System;

namespace Domain.Entities
{
    /// <summary>
    /// 客服微信图片表，对应 tk_service_image。
    /// agent_userid=0 表示系统客服图片；非 0 表示对应代理的客服图片。
    /// </summary>
    public sealed class TkServiceImage
    {
        public TkServiceImage(string imageUrl, long agentUserid)
        {
            ImageUrl = imageUrl;
            AgentUserid = agentUserid;
            CreateTime = DateTimeOffset.Now;
        }

        public int ImageId { get; private set; }

        /// <summary>客服图片路径/URL。</summary>
        public string ImageUrl { get; private set; }

        /// <summary>代理用户 id；0 表示系统客服。</summary>
        public long AgentUserid { get; private set; }

        public DateTimeOffset CreateTime { get; private set; }

        /// <summary>更新图片 URL。</summary>
        public void UpdateImageUrl(string imageUrl)
        {
            ImageUrl = imageUrl;
        }
    }
}
