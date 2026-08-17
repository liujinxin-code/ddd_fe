using Application.Events.Agent.Contracts.Commands;
using FluentValidation;

namespace Application.Events.Agent.Validators
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
