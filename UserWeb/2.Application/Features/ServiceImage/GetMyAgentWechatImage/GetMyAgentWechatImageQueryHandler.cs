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
    public class GetMyAgentWechatImageQueryHandler(
            ITkUserRepository tkUserRepository,
            IServiceImageRepository serviceImageRepository,
            ICurrentUser currentUser
        ) : IRequestHandler<GetMyAgentWechatImageQuery, AgentWechatImageResponse?>
    {
        public async Task<AgentWechatImageResponse?> Handle(GetMyAgentWechatImageQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                throw new UnauthorizedDomainException();
            }

            var user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null) throw new BusinessException("用户不存在");

            long targetAgentUserId = 0;

            // 代理看自己系统客服；普通用户优先看上级代理，否则系统客服。
            if (!user.IsAgentFnc() && user.AgentUserid > 0)
            {
                targetAgentUserId = user.AgentUserid;
            }

            var image = await serviceImageRepository.GetByAgentUserIdAsync(targetAgentUserId, ct);

            // 上级代理未上传， fallback 到系统客服
            if (image == null && targetAgentUserId != 0)
            {
                image = await serviceImageRepository.GetByAgentUserIdAsync(0, ct);
            }

            return new AgentWechatImageResponse { ImageUrl = image?.ImageUrl ?? string.Empty };
        }
    }
}
