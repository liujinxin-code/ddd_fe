using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Response.ConsumeLog;
using Application.Events.ConsumeLogs.Contracts.Queries;
using MediatR;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.ConsumeLogs.Handlers.Queries
{
    /// <summary>
    /// 「消费流水」分页查询。只读当前登录用户自己的余额变动（用户id 来自 ICurrentUser，杜绝越权查询他人流水）。
    /// </summary>
    public class GetConsumeLogsQueryHandler(
        IConsumeLogRepository consumeLogRepository,
        ICurrentUser currentUser)
        : IRequestHandler<GetConsumeLogsQuery, ApiResult<List<ConsumeLogResponse>>>
    {
        public async Task<ApiResult<List<ConsumeLogResponse>>> Handle(GetConsumeLogsQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                return new ApiResult<List<ConsumeLogResponse>>
                {
                    Code = 401,
                    Message = "登录失效",
                    Data = new List<ConsumeLogResponse>(),
                    DataTotal = 0
                };
            }

            string sortField;
            bool sortDesc;
            if (string.IsNullOrWhiteSpace(query.Sorting))
            {
                sortField = "createtime";
                sortDesc = true;
            }
            else
            {
                var parts = query.Sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                sortField = parts[0];
                sortDesc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }

            var (items, total) = await consumeLogRepository.GetPagedByUserAsync(
                (int)currentUser.Userid,
                query.ConsumeStatus,
                query.Keyword,
                query.PageIndex,
                query.PageSize,
                sortField,
                sortDesc,
                ct);

            // 金额统一按 decimal(11,6) 精度回传，避免前端拿到未规整的小数尾巴。
            var list = items.Select(o =>
            {
                o.BeforeAmount = Utils.RoundToSixDecimals(o.BeforeAmount);
                o.AfterAmount = Utils.RoundToSixDecimals(o.AfterAmount);
                o.ChangeAmount = Utils.RoundToSixDecimals(o.ChangeAmount);
                return o;
            }).ToList();

            return new ApiResult<List<ConsumeLogResponse>>
            {
                Code = 200,
                Message = "Success!",
                Data = list,
                DataTotal = total
            };
        }
    }
}
