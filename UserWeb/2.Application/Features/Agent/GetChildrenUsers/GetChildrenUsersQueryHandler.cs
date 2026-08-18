using Application.Abstractions.Repositories;
using Shared.Exceptions;

using Application.Features.Agent.Models;
using Application.Common.Models;
using Application.Features.Agent;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public class GetChildrenUsersQueryHandler(ITkUserRepository tkUserRepository, ICurrentUser currentUser)
        : IRequestHandler<GetChildrenUsersQuery, PagedResult<ChildrenUserResponse>>
    {
        public async Task<PagedResult<ChildrenUserResponse>> Handle(GetChildrenUsersQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                throw new UnauthorizedDomainException();
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

            // 返回中性分页载体 PagedResult，真实总条数 total 由 TotalCount 携带；
            // HTTP 边缘层（ApiPaged）会显式构造信封，避免 IList 被 ApiResult.Successed 按 Count 覆盖总条数。
            return new PagedResult<ChildrenUserResponse>(list, total);
        }
    }
}
