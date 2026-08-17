using Application.Features.Agent;
using FluentValidation;

namespace Application.Features.Agent
{
    public class UpsertAgentOverallPriceCommandValidator : AbstractValidator<UpsertAgentOverallPriceCommand>
    {
        public UpsertAgentOverallPriceCommandValidator()
        {
            RuleFor(x => x.OverallPercent)
                .InclusiveBetween(0, 200)
                .WithMessage("总体加价百分比须在 [0,200] 之间");
        }
    }
}
