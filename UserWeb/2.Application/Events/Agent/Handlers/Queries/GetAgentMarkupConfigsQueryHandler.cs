using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Response.Agent;
using Application.Events.Agent.Contracts.Queries;
using Domain.Enums;
using MediatR;
using Shared.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Queries
{
    /// <summary>
    /// 使用 IConfigRepository 读取配置，使用 IAgentPricingRepository 读取当前代理已加价的 configId，
    /// 排除已加价的配置后返回可选列表。
    /// </summary>
    public class GetAgentMarkupConfigsQueryHandler(
        IConfigRepository configRepository,
        IAgentPricingRepository agentPricingRepository,
        ICurrentUser currentUser)
        : IRequestHandler<GetAgentMarkupConfigsQuery, ApiResult<List<AgentMarkupConfigResponse>>>
    {
        public async Task<ApiResult<List<AgentMarkupConfigResponse>>> Handle(GetAgentMarkupConfigsQuery query, CancellationToken ct)
        {
            // 获取当前代理已加价的 configId 集合。
            var existingMarkupConfigIds = await agentPricingRepository.GetMarkupConfigIdsByAgentAsync(currentUser.Userid, ct);

            // 读取指定平台+业务类型下、前台可见、且未加价的配置。
            var (configs, _) = await configRepository.GetConfigsAsync(
                query.PlatformId,
                query.SubPlatformId,
                query.PageIndex,
                query.PageSize,
                "configsort",
                false,
                keyword: null,
                ct);

            var availableConfigs = configs
                .Where(c => c.ConfigStatus == ConfigStatus.AllEnabled && !existingMarkupConfigIds.Contains(c.ConfigId))
                .ToList();

            int availableTotal = availableConfigs.Count;

            if (availableConfigs.Count == 0)
            {
                return ApiResult<List<AgentMarkupConfigResponse>>.Successed(new List<AgentMarkupConfigResponse>(), 0);
            }

            var configIds = availableConfigs.Select(c => c.ConfigId).ToList();
            var agentCustom = await configRepository.GetAgentCustomPricesAsync(currentUser.Userid, configIds, ct);

            var list = availableConfigs.Select(c =>
            {
                decimal basePrice = agentCustom.TryGetValue(c.ConfigId, out var custom)
                    ? custom
                    : c.ConfigPrice;

                return new AgentMarkupConfigResponse
                {
                    ConfigId = c.ConfigId,
                    ConfigName = c.ConfigName,
                    ConfigNotice = c.ConfigNotice,
                    BasePrice = Utils.RoundToSixDecimals(basePrice),
                    ShowPriceUnit = c.ShowPriceUnit,
                    MinQuantity = c.MinQuantity,
                    MaxQuantity = c.MaxQuantity,
                    OrderUnit = c.OrderUnit
                };
            }).ToList();

            return new ApiResult<List<AgentMarkupConfigResponse>>
            {
                Code = 200,
                Message = "Success!",
                Data = list,
                DataTotal = availableTotal
            };
        }
    }
}
