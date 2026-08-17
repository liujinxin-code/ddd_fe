namespace Application.Common.Models.Enum
{
    /// <summary>
    /// 枚举同步选项：value/name/label 三元组，供前端下拉框动态渲染。
    /// </summary>
    public class EnumOption
    {
        /// <summary>枚举 int 值</summary>
        public int Value { get; set; }

        /// <summary>枚举成员名称（如 Running）</summary>
        public string Name { get; set; } = default!;

        /// <summary>枚举显示文本（取自 DescriptionAttribute）</summary>
        public string Label { get; set; } = default!;
    }
}
