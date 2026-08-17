using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Events.Agent.Contracts.Commands;
using MediatR;
using Shared.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Commands
{
    public class DeleteAgentMarkupCommandHandler(
        IAgentPricingRepository agentPricingRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
        : IRequestHandler<DeleteAgentMarkupCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(DeleteAgentMarkupCommand request, CancellationToken ct)
        {
            var existing = await agentPricingRepository.GetMarkupAsync(request.ConfigId, currentUser.Userid, ct);
            if (existing is null)
            {
                throw new BusinessException("该配置加价记录不存在");
            }

            agentPricingRepository.DeleteMarkup(existing);
            await unitOfWork.SaveChangesAsync(ct);
            return ApiResult.Successed();
        }
    }
}
