using Application.Features.Agent;
using FluentValidation;

namespace Application.Features.Agent
{
    public class GetAgentMarkupConfigsQueryValidator : AbstractValidator<GetAgentMarkupConfigsQuery>
    {
        public GetAgentMarkupConfigsQueryValidator()
        {
            RuleFor(x => x.PlatformId)
                .GreaterThan(0).WithMessage("平台id必须大于0");

            RuleFor(x => x.SubPlatformId)
                .GreaterThan(0).WithMessage("业务类型id必须大于0");

            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("页码必须大于0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("每页条数须在 1-100 之间");
        }
    }
}
