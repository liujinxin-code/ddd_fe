using Application.Events.Agent.Contracts.Commands;
using FluentValidation;

namespace Application.Events.Agent.Validators
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
