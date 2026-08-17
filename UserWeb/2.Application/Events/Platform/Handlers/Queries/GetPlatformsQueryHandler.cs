using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Response.Platform;
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
        : IRequestHandler<GetPlatformsQuery, ApiResult<List<PlatformResponse>>>
    {
        public async Task<ApiResult<List<PlatformResponse>>> Handle(GetPlatformsQuery query, CancellationToken ct)
        {
            var platforms = await platformRepository.GetPlatformsAsync(ct);
            var list = platforms.Select(p => new PlatformResponse
            {
                PlatformId = p.PlatformId,
                PlatformName = p.PlatformName ?? string.Empty
            }).ToList();
            return ApiResult<List<PlatformResponse>>.Successed(list);
        }
    }
}
