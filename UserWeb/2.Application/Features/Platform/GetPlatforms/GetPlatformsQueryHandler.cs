using Application.Abstractions.Repositories;

using Application.Features.Platform.Models;
using Application.Features.Platform;
using Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Platform
{
    public class GetPlatformsQueryHandler(IPlatformRepository platformRepository)
        : IRequestHandler<GetPlatformsQuery, List<PlatformResponse>>
    {
        public async Task<List<PlatformResponse>> Handle(GetPlatformsQuery query, CancellationToken ct)
        {
            var platforms = await platformRepository.GetPlatformsAsync(ct);
            var list = platforms.Select(p => new PlatformResponse
            {
                PlatformId = p.PlatformId,
                PlatformName = p.PlatformName ?? string.Empty
            }).ToList();
            return list;
        }
    }
}
