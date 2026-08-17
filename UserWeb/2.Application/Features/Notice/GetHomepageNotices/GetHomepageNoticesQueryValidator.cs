using Application.Features.Notice;
using FluentValidation;

namespace Application.Features.Notice
{
    public class GetHomepageNoticesQueryValidator : AbstractValidator<GetHomepageNoticesQuery>
    {
        public GetHomepageNoticesQueryValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(1).WithMessage("页码必须大于等于 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("每页条数需在 1-100 之间");

            // 首页公告业务排序固定（置顶优先 + 创建时间倒序），Sorting 不参与，此处不予校验。
        }
    }
}
