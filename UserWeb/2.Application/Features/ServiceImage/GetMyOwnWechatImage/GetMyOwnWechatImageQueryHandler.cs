using Application.Abstractions;
using Application.Common.Models;
using Application.Abstractions.Repositories;

using Application.Features.ServiceImage.Models;
using Application.Features.ServiceImage;
using MediatR;
using Shared.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ServiceImage
{
    public class GetMyOwnWechatImageQueryHandler(
            IServiceImageRepository serviceImageRepository,
            ICurrentUser currentUser
        ) : IRequestHandler<GetMyOwnWechatImageQuery, AgentWechatImageResponse?>
    {
        public async Task<AgentWechatImageResponse?> Handle(GetMyOwnWechatImageQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                throw new UnauthorizedDomainException();
            }

            var image = await serviceImageRepository.GetByAgentUserIdAsync(currentUser.Userid, ct);

            return new AgentWechatImageResponse { ImageUrl = image?.ImageUrl ?? string.Empty };
        }
    }
}
