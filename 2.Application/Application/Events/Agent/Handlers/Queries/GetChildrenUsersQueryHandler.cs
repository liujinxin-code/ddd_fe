using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Response.Agent;
using Application.Events.Agent.Contracts.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Queries
{
    public class GetChildrenUsersQueryHandler(ITkUserRepository tkUserRepository, ICurrentUser currentUser)
        : IRequestHandler<GetChildrenUsersQuery, ApiResult<List<ChildrenUserResponse>>>
    {
        public async Task<ApiResult<List<ChildrenUserResponse>>> Handle(GetChildrenUsersQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                return new ApiResult<List<ChildrenUserResponse>>
                {
                    Code = 401,
                    Message = "登录失效",
                    Data = new List<ChildrenUserResponse>(),
                    DataTotal = 0
                };
            }

            // 解析排序：格式 "字段 [asc|desc]"，缺省按 userid 倒序（最新创建的在前）。
            string sortField;
            bool sortDesc;
            if (string.IsNullOrWhiteSpace(query.Sorting))
            {
                sortField = "userid";
                sortDesc = true;
            }
            else
            {
                var parts = query.Sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                sortField = parts[0];
                sortDesc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }

            var (items, total) = await tkUserRepository.GetChildrenByAgentAsync(
                currentUser.Userid, query.PageIndex, query.PageSize, query.Keyword, sortField, sortDesc, query.UserStatus, ct);

            var list = items.Select(t => new ChildrenUserResponse
            {
                Userid = t.Userid,
                Username = t.Username,
                Email = t.Email,
                UserAmount = t.UserAmount,
                UserStatus = t.UserStatus,
                AgentUserid = t.AgentUserid,
                Createby = t.Createby,
                CreateTime = t.CreateTime
            }).ToList();

            // data 为 List（IList），ApiResult.Successed 会按 list.Count 回填 DataTotal，
            // 故此处显式构造，保留真实总条数 total 供前端分页（页索引/页大小已在请求中，无需回显）。
            return new ApiResult<List<ChildrenUserResponse>>
            {
                Code = 200,
                Message = "Success!",
                Data = list,
                DataTotal = total
            };
        }
    }
}
