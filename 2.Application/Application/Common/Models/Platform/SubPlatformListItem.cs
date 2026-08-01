namespace Application.Common.Models.Platform
{
    /// <summary>
    /// 子平台列表项。用于二级联动下拉（id + name）。
    /// </summary>
    public class SubPlatformListItem
    {
        /// <summary>
        /// 子平台id
        /// </summary>
        public int SubPlatformId { get; set; }

        /// <summary>
        /// 子平台名称
        /// </summary>
        public string SubPlatformName { get; set; } = string.Empty;

        /// <summary>
        /// 侧边栏公告：选中该子平台时展示给用户
        /// </summary>
        public string SubPlatformNotice { get; set; } = string.Empty;
    }
}
