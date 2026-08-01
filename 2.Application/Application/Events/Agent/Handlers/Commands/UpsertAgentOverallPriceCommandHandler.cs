using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Events.Agent.Contracts.Commands;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Commands
{
    public class UpsertAgentOverallPriceCommandHandler(
        IAgentPricingRepository agentPricingRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpsertAgentOverallPriceCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(UpsertAgentOverallPriceCommand request, CancellationToken ct)
        {
            var existing = await agentPricingRepository.GetOverallByUserAsync(request.UserId, ct);
            if (existing is null)
            {
                var entity = new TkPriceOverall(request.UserId, request.OverallPercent);
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
