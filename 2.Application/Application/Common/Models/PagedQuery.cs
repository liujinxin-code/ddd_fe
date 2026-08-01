namespace Application.Common.Models
{
    /// <summary>
    /// 分页查询基类：所有支持“分页 + 排序”的查询可继承此基类，
    /// 复用 PageIndex / PageSize / Sorting 三个公共参数及其默认值，避免每个查询重复声明。
    /// Sorting 形如 "configprice desc" / "configsort asc"，具体字段白名单由各自 Validator 校验。
    /// 前端分页参数随请求传入，响应无需回显页码/页大小（详见各查询的返回模型）。
    /// </summary>
    public abstract record class PagedQuery
    {
        /// <summary>页索引，从 1 开始，缺省 1</summary>
        public int PageIndex { get; init; } = 1;

        /// <summary>页大小，缺省 20</summary>
        public int PageSize { get; init; } = 20;

        /// <summary>排序表达式，缺省 null（由具体查询决定默认排序）</summary>
        public string? Sorting { get; init; }
    }
}
