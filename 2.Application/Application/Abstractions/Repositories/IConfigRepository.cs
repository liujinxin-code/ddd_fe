using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Repositories
{
    /// <summary>
    /// 业务配置与定价只读仓储（前台首页展示业务+价格使用，写操作在后台完成，前台不提供）。
    /// 价格数据（单独定价/总体加价/单业务加价）仅用于“读取并计算当前用户看到的价格”，不做任何写。
    /// </summary>
    public interface IConfigRepository
    {
        /// <summary>
        /// 按 平台 + 业务类型 过滤，仅取前台可见（config_status=1 全部启用）的业务配置，分页并返回总数。
        /// 排序字段来自白名单（configid/configname/configprice/configsort/minquantity/maxquantity/createtime），缺省按 config_sort 升序。
        /// </summary>
        Task<(IReadOnlyList<TkConfig> Items, int Total)> GetConfigsAsync(
            int platformId, int subPlatformId, int pageIndex, int pageSize,
            string? sortField, bool sortDesc, string? keyword = null, CancellationToken ct = default);

        /// <summary>
        /// 批量获取某用户（userid）在给定 config 集合上的单独定价，返回 configId -> custom_price 字典（仅含存在记录的行）。
        /// </summary>
        Task<Dictionary<int, decimal>> GetUserCustomPricesAsync(long userId, IEnumerable<int> configIds, CancellationToken ct = default);

        /// <summary>
        /// 批量获取某代理（agentUserId）在给定 config 集合上的单独定价（代理“进货价”来源）。
        /// </summary>
        Task<Dictionary<int, decimal>> GetAgentCustomPricesAsync(long agentUserId, IEnumerable<int> configIds, CancellationToken ct = default);

        /// <summary>
        /// 批量获取某代理在给定 config 集合上的单业务加价金额，返回 configId -> markup_add_price 字典。
        /// </summary>
        Task<Dictionary<int, decimal>> GetAgentMarkupsAsync(long agentUserId, IEnumerable<int> configIds, CancellationToken ct = default);

        /// <summary>
        /// 获取某代理的总体加价配置（按 userid 唯一），不存在返回 null。
        /// </summary>
        Task<TkPriceOverall?> GetAgentOverallAsync(long agentUserId, CancellationToken ct = default);

        /// <summary>
        /// 按 config_id 列表批量加载业务配置（不限状态，可用性由调用方校验），用于下单时解析价格与渠道。
        /// </summary>
        Task<IReadOnlyList<TkConfig>> GetByIdsAsync(IEnumerable<int> configIds, CancellationToken ct = default);
    }
}
