using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Platform;
using Application.Events.Platform.Contracts.Queries;
using Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Platform.Handlers.Queries
{
    public class GetPlatformsQueryHandler(IPlatformRepository platformRepository)
        : IRequestHandler<GetPlatformsQuery, ApiResult<List<PlatformListItem>>>
    {
        public async Task<ApiResult<List<PlatformListItem>>> Handle(GetPlatformsQuery query, CancellationToken ct)
        {
            var platforms = await platformRepository.GetPlatformsAsync(ct);
            var list = platforms.Select(p => new PlatformListItem
            {
                PlatformId = p.PlatformId,
                PlatformName = p.PlatformName ?? string.Empty
            }).ToList();
            return ApiResult<List<PlatformListItem>>.Successed(list);
        }
    }
}
