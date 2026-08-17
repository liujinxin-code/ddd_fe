using Application.Features.Agent;
using FluentValidation;

namespace Application.Features.Agent
{
    public class UpsertAgentMarkupCommandValidator : AbstractValidator<UpsertAgentMarkupCommand>
    {
        public UpsertAgentMarkupCommandValidator()
        {
            RuleFor(x => x.ConfigId)
                .GreaterThan(0).WithMessage("配置id必须大于0");

            RuleFor(x => x.MarkupAddPrice)
                .GreaterThanOrEqualTo(0).WithMessage("加价金额不能为负");
        }
    }
}
