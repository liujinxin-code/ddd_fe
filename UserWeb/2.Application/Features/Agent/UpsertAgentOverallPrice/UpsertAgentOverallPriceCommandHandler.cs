using Application.Abstractions;
using Application.Common.Models;
using Application.Abstractions.Repositories;

using Application.Features.Agent;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public class UpsertAgentOverallPriceCommandHandler(
        IAgentPricingRepository agentPricingRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
        : IRequestHandler<UpsertAgentOverallPriceCommand, Unit>
    {
        public async Task<Unit> Handle(UpsertAgentOverallPriceCommand request, CancellationToken ct)
        {
            var existing = await agentPricingRepository.GetOverallByUserAsync(currentUser.Userid, ct);
            if (existing is null)
            {
                var entity = new TkPriceOverall(currentUser.Userid, request.OverallPercent);
                await agentPricingRepository.AddOverallAsync(entity, ct);
            }
            else
            {
                existing.UpdatePercent(request.OverallPercent);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
