using Application.Abstractions;
using Application.Common.Models;
using Application.Abstractions.Repositories;

using Application.Features.Agent;
using MediatR;
using Shared.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public class DeleteAgentMarkupCommandHandler(
        IAgentPricingRepository agentPricingRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
        : IRequestHandler<DeleteAgentMarkupCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteAgentMarkupCommand request, CancellationToken ct)
        {
            var existing = await agentPricingRepository.GetMarkupAsync(request.ConfigId, currentUser.Userid, ct);
            if (existing is null)
            {
                throw new BusinessException("该配置加价记录不存在");
            }

            agentPricingRepository.DeleteMarkup(existing);
            await unitOfWork.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
