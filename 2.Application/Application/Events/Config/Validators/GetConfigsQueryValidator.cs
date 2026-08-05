using Application.Events.Config.Contracts.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Config.Validators
{
    public class GetConfigsQueryValidator : AbstractValidator<GetConfigsQuery>
    {
        // 允许排序的字段白名单（与仓储 ApplyConfigSorting 保持一致，小写无空格）
        private static readonly HashSet<string> SortFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "configid", "configname", "configprice", "configsort",
            "minquantity", "maxquantity", "createtime"
        };

        public GetConfigsQueryValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.PlatformId)
                .GreaterThan(0).WithMessage("平台id必须大于0");

            RuleFor(x => x.SubPlatformId)
                .GreaterThan(0).WithMessage("业务类型id必须大于0");

            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(1).WithMessage("页码必须大于等于 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("每页条数需在 1-100 之间");

            RuleFor(x => x.Sorting)
                .Must(BeValidSorting).WithMessage("排序字段不合法")
                .When(x => !string.IsNullOrWhiteSpace(x.Sorting));

            RuleFor(x => x.Keyword)
                .MaximumLength(50).WithMessage("搜索关键词长度不能超过 50")
                .When(x => !string.IsNullOrWhiteSpace(x.Keyword));
        }

        private static bool BeValidSorting(string? sorting)
        {
            if (string.IsNullOrWhiteSpace(sorting))
            {
                return true;
            }

            var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 2)
            {
                return false;
            }

            if (!SortFields.Contains(parts[0]))
            {
                return false;
            }

            if (parts.Length == 2 &&
                !parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                !parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
