using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Features.Config.Models;
using Application.Features.Config;
using Application.Services;
using Domain.Entities;
using MediatR;
using Shared.Utilities;

namespace Application.Features.Config
{
    /// <summary>
    /// API 文档业务配置精简列表查询。与首页 list 共用定价计算逻辑，但返回字段更精简，不影响现有接口。
    /// </summary>
    public class GetApiConfigsQueryHandler(
        IConfigRepository configRepository,
        ITkUserRepository userRepository,
        ICurrentUser currentUser)
        : IRequestHandler<GetApiConfigsQuery, ApiResult<List<ConfigApiResponse>>>
    {
        public async Task<ApiResult<List<ConfigApiResponse>>> Handle(GetApiConfigsQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                return new ApiResult<List<ConfigApiResponse>>
                {
                    Code = 401,
                    Message = "登录失效",
                    Data = new List<ConfigApiResponse>(),
                    DataTotal = 0
                };
            }

            string sortField;
            bool sortDesc;
            if (string.IsNullOrWhiteSpace(query.Sorting))
            {
                sortField = "configsort";
                sortDesc = false;
            }
            else
            {
                var parts = query.Sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                sortField = parts[0];
                sortDesc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }

            var (configs, total) = await configRepository.GetConfigsAsync(
                query.PlatformId, query.SubPlatformId, query.PageIndex, query.PageSize, sortField, sortDesc, query.Keyword, ct);

            if (configs.Count == 0)
            {
                return ApiResult<List<ConfigApiResponse>>.Successed(new List<ConfigApiResponse>(), 0);
            }

            var user = await userRepository.GetByIdAsNoTrackingAsync(currentUser.Userid, ct);
            if (user is null)
            {
                return new ApiResult<List<ConfigApiResponse>>
                {
                    Code = 400,
                    Message = "用户不存在",
                    Data = new List<ConfigApiResponse>(),
                    DataTotal = 0
                };
            }

            long agentUserId = user.AgentUserid;
            bool hasAgent = agentUserId > 0;

            var configIds = configs.Select(c => c.ConfigId).ToList();
            var userCustom = await configRepository.GetUserCustomPricesAsync(currentUser.Userid, configIds, ct);

            Dictionary<int, decimal> agentCustom = new();
            Dictionary<int, decimal> agentMarkup = new();
            TkPriceOverall? overall = null;
            if (hasAgent)
            {
                agentCustom = await configRepository.GetAgentCustomPricesAsync(agentUserId, configIds, ct);
                agentMarkup = await configRepository.GetAgentMarkupsAsync(agentUserId, configIds, ct);
                overall = await configRepository.GetAgentOverallAsync(agentUserId, ct);
            }

            var priceContext = new UserPriceContext
            {
                HasAgent = hasAgent,
                UserCustom = userCustom,
                AgentCustom = agentCustom,
                AgentMarkup = agentMarkup,
                HasOverall = overall != null,
                OverallPercent = overall?.OverallPercent ?? 0
            };

            var items = configs.Select(c =>
            {
                var unitPrice = ConfigPriceCalculator.CalculateUnitPrice(c, priceContext);
                return new ConfigApiResponse
                {
                    ConfigId = c.ConfigId,
                    ConfigName = c.ConfigName,
                    UnitPrice = unitPrice,
                    MinQuantity = c.MinQuantity,
                    MaxQuantity = c.MaxQuantity,
                    OrderUnit = c.OrderUnit,
                    JsonTemplate = c.JsonTemplate
                };
            }).ToList();

            return new ApiResult<List<ConfigApiResponse>>
            {
                Code = 200,
                Message = "Success!",
                Data = items,
                DataTotal = total
            };
        }
    }
}
