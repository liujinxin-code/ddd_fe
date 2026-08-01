namespace Application.Common.Models.Platform
{
    /// <summary>
    /// 平台列表项。仅暴露下拉展示所需字段（id + name），不含 logo 等无关数据。
    /// </summary>
    public class PlatformListItem
    {
        /// <summary>
        /// 平台id
        /// </summary>
        public int PlatformId { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;
    }
}
