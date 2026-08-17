using Application.Features.Platform;
using FluentValidation;

namespace Application.Features.Platform
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
