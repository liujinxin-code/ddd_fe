using Application.Events.Platform.Contracts.Queries;
using FluentValidation;

namespace Application.Events.Platform.Validators
{
    public class GetSubPlatformsByPlatformQueryValidator : AbstractValidator<GetSubPlatformsByPlatformQuery>
    {
        public GetSubPlatformsByPlatformQueryValidator()
        {
            RuleFor(x => x.PlatformId)
                .GreaterThan(0).WithMessage("平台id必须大于0");
        }
    }
}
