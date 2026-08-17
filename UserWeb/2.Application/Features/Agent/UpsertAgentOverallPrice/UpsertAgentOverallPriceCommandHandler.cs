using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
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
        : IRequestHandler<UpsertAgentOverallPriceCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(UpsertAgentOverallPriceCommand request, CancellationToken ct)
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
            return ApiResult.Successed();
        }
    }
}
