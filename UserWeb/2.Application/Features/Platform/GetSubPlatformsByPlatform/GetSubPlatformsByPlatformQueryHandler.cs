using Application.Abstractions.Repositories;

using Application.Features.Platform.Models;
using Application.Features.Platform;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Platform
{
    public class GetSubPlatformsByPlatformQueryHandler(IPlatformRepository platformRepository)
        : IRequestHandler<GetSubPlatformsByPlatformQuery, List<SubPlatformResponse>>
    {
        public async Task<List<SubPlatformResponse>> Handle(GetSubPlatformsByPlatformQuery query, CancellationToken ct)
        {
            var subs = await platformRepository.GetSubsByPlatformAsync(query.PlatformId, ct);
            var list = subs.Select(s => new SubPlatformResponse
            {
                SubPlatformId = s.SubPlatformId,
                SubPlatformName = s.SubPlatformName ?? string.Empty,
                SubPlatformNotice = s.SubPlatformNotice ?? string.Empty
            }).ToList();
            return list;
        }
    }
}
