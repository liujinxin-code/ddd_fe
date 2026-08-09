using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Response.ServiceImage;
using Application.Events.ServiceImage.Contracts.Queries;
using MediatR;
using Shared.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.ServiceImage.Handlers.Queries
{
    public class GetMyOwnWechatImageQueryHandler(
            IServiceImageRepository serviceImageRepository,
            ICurrentUser currentUser
        ) : IRequestHandler<GetMyOwnWechatImageQuery, ApiResult<AgentWechatImageResponse>>
    {
        public async Task<ApiResult<AgentWechatImageResponse>> Handle(GetMyOwnWechatImageQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                return new ApiResult<AgentWechatImageResponse>
                {
                    Code = 401,
                    Message = "登录失效",
                    Data = new AgentWechatImageResponse { ImageUrl = string.Empty, AgentUserid = 0 },
                    DataTotal = 0
                };
            }

            var image = await serviceImageRepository.GetByAgentUserIdAsync(currentUser.Userid, ct);

            if (image == null)
            {
                return new ApiResult<AgentWechatImageResponse>
                {
                    Code = 200,
                    Message = "Success!",
                    Data = new AgentWechatImageResponse { ImageUrl = string.Empty, AgentUserid = currentUser.Userid },
                    DataTotal = 0
                };
            }

            return new ApiResult<AgentWechatImageResponse>
            {
                Code = 200,
                Message = "Success!",
                Data = new AgentWechatImageResponse { ImageUrl = image.ImageUrl, AgentUserid = currentUser.Userid },
                DataTotal = 1
            };
        }
    }
}
