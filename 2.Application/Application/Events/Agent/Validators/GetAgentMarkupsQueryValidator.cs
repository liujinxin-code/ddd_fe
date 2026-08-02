using Application.Events.Agent.Contracts.Queries;
using FluentValidation;

namespace Application.Events.Agent.Validators
{
    public class GetAgentMarkupsQueryValidator : AbstractValidator<GetAgentMarkupsQuery>
    {
        public GetAgentMarkupsQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("页码必须大于0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("每页条数须在 1-100 之间");
        }
    }
}
