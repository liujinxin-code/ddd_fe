using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Platform;
using Application.Events.Platform.Contracts.Queries;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Platform.Handlers.Queries
{
    public class GetSubPlatformsByPlatformQueryHandler(IPlatformRepository platformRepository)
        : IRequestHandler<GetSubPlatformsByPlatformQuery, ApiResult<List<SubPlatformListItem>>>
    {
        public async Task<ApiResult<List<SubPlatformListItem>>> Handle(GetSubPlatformsByPlatformQuery query, CancellationToken ct)
        {
            var subs = await platformRepository.GetSubsByPlatformAsync(query.PlatformId, ct);
            var list = subs.Select(s => new SubPlatformListItem
            {
                SubPlatformId = s.SubPlatformId,
                SubPlatformName = s.SubPlatformName ?? string.Empty,
                SubPlatformNotice = s.SubPlatformNotice ?? string.Empty
            }).ToList();
            return ApiResult<List<SubPlatformListItem>>.Successed(list);
        }
    }
}
