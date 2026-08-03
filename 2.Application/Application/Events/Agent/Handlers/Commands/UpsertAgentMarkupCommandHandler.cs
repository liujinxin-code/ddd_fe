using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Events.Agent.Contracts.Commands;
using Domain.Entities;
using MediatR;
using Shared.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Commands
{
    public class UpsertAgentMarkupCommandHandler(
        IAgentPricingRepository agentPricingRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
        : IRequestHandler<UpsertAgentMarkupCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(UpsertAgentMarkupCommand request, CancellationToken ct)
        {
            decimal price = Utils.RoundToSixDecimals(request.MarkupAddPrice);

            var existing = await agentPricingRepository.GetMarkupAsync(request.ConfigId, currentUser.Userid, ct);
            if (existing is null)
            {
                // tk_price_agent_markup.agent_userid 已改为 bigint，与 CurrentUser.Userid(long) 一致，无需转换。
                var entity = new TkPriceAgentMarkup(request.ConfigId, currentUser.Userid, price);
                await agentPricingRepository.AddMarkupAsync(entity, ct);
            }
            else
            {
                existing.UpdateMarkup(price);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return ApiResult.Successed();
        }
    }
}
