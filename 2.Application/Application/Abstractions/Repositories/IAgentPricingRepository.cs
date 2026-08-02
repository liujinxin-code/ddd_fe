using Application.Abstractions;
using Application.Common.Models.Agent;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Repositories
{
    /// <summary>
    /// 代理自控价格写仓储：tk_price_overall（总体加价，按 userid 唯一）与
    /// tk_price_agent_markup（单业务加价，按 config_id + agent_userid）。
    /// 仅允许操作“当前代理自己”的数据；UserId / AgentUserId 必须由调用方从当前登录用户注入，
    /// 仓储本身不校验归属，归属校验由调用层（Controller 注入 CurrentUser）保证，杜绝越权。
    /// 查询方法返回被 EF 追踪的实体，便于修改后由 IUnitOfWork.SaveChangesAsync 持久化。
    /// </summary>
    public interface IAgentPricingRepository
    {
        /// <summary>按代理用户id 获取总体加价记录（被追踪）。</summary>
        Task<TkPriceOverall?> GetOverallByUserAsync(long userId, CancellationToken ct = default);

        /// <summary>新增一条总体加价记录。</summary>
        Task<bool> AddOverallAsync(TkPriceOverall entity, CancellationToken ct = default);

        /// <summary>按 config_id + agent_userid 获取单业务加价记录（被追踪）。</summary>
        Task<TkPriceAgentMarkup?> GetMarkupAsync(int configId, long agentUserId, CancellationToken ct = default);

        /// <summary>新增一条单业务加价记录。</summary>
        Task<bool> AddMarkupAsync(TkPriceAgentMarkup entity, CancellationToken ct = default);

        /// <summary>删除一条单业务加价记录。</summary>
        void DeleteMarkup(TkPriceAgentMarkup entity, CancellationToken ct = default);

        /// <summary>
        /// 分页获取某代理的单业务加价记录，并与 tk_config 关联，支持按业务名称模糊检索。
        /// 返回加价记录与对应配置的联合读取模型及总条数。
        /// </summary>
        Task<(IReadOnlyList<AgentMarkupWithConfig> Items, int Total)> GetMarkupsByAgentAsync(
            long agentUserId, int pageIndex, int pageSize, string? keyword, CancellationToken ct = default);

        /// <summary>
        /// 获取某代理已加价的 configId 集合（用于新增时排除已存在记录）。
        /// </summary>
        Task<IReadOnlyList<int>> GetMarkupConfigIdsByAgentAsync(long agentUserId, CancellationToken ct = default);
    }
}
