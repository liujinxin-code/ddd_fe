using Application.Features.Agent;
using FluentValidation;

namespace Application.Features.Agent
{
    public class DeleteAgentMarkupCommandValidator : AbstractValidator<DeleteAgentMarkupCommand>
    {
        public DeleteAgentMarkupCommandValidator()
        {
            RuleFor(x => x.ConfigId)
                .GreaterThan(0).WithMessage("配置id必须大于0");
        }
    }
}
