using Application.Events.Agent.Contracts.Commands;
using FluentValidation;

namespace Application.Events.Agent.Validators
{
    public class WithdrawAgentAmountCommandValidator : AbstractValidator<WithdrawAgentAmountCommand>
    {
        public WithdrawAgentAmountCommandValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("提取金额必须大于0");
        }
    }
}
