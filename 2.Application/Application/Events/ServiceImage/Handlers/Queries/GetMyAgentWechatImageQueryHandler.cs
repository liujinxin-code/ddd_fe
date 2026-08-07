using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.ServiceImage;
using Application.Events.ServiceImage.Contracts.Queries;
using MediatR;
using Shared.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.ServiceImage.Handlers.Queries
{
    public class GetMyAgentWechatImageQueryHandler(
            ITkUserRepository tkUserRepository,
            IServiceImageRepository serviceImageRepository,
            ICurrentUser currentUser
        ) : IRequestHandler<GetMyAgentWechatImageQuery, ApiResult<AgentWechatImageItem>>
    {
        public async Task<ApiResult<AgentWechatImageItem>> Handle(GetMyAgentWechatImageQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                return new ApiResult<AgentWechatImageItem>
                {
                    Code = 401,
                    Message = "登录失效",
                    Data = new AgentWechatImageItem { ImageUrl = string.Empty, AgentUserid = 0 },
                    DataTotal = 0
                };
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
                targetAgentUserId = 0;
            }

            if (image == null)
            {
                return new ApiResult<AgentWechatImageItem>
                {
                    Code = 200,
                    Message = "Success!",
                    Data = new AgentWechatImageItem { ImageUrl = string.Empty, AgentUserid = 0 },
                    DataTotal = 0
                };
            }

            return new ApiResult<AgentWechatImageItem>
            {
                Code = 200,
                Message = "Success!",
                Data = new AgentWechatImageItem { ImageUrl = image.ImageUrl, AgentUserid = targetAgentUserId },
                DataTotal = 1
            };
        }
    }
}
