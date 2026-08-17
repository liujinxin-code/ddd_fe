namespace Application.Features.ServiceImage.Models
{
    /// <summary>
    /// 当前用户应展示的客服微信图片。
    /// </summary>
    public class AgentWechatImageResponse
    {
        /// <summary>图片 URL。</summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>图片归属：0=系统客服，非0=代理用户id。</summary>
        public long AgentUserid { get; set; }
    }
}
