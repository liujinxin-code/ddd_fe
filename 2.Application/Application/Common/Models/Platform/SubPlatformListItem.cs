namespace Application.Common.Models.Platform
{
    /// <summary>
    /// 业务类型列表项。用于二级联动下拉（id + name）。
    /// </summary>
    public class SubPlatformListItem
    {
        /// <summary>
        /// 业务类型id
        /// </summary>
        public int SubPlatformId { get; set; }

        /// <summary>
        /// 业务类型名称
        /// </summary>
        public string SubPlatformName { get; set; } = string.Empty;

        /// <summary>
        /// 侧边栏公告：选中该业务类型时展示给用户
        /// </summary>
        public string SubPlatformNotice { get; set; } = string.Empty;
    }
}
