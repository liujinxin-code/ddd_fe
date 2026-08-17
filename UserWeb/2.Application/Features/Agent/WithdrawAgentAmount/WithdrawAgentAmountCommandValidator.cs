using Application.Features.Agent;
using FluentValidation;

namespace Application.Features.Agent
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
