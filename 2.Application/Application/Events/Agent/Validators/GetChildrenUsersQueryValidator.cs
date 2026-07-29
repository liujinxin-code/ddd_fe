using Application.Events.Agent.Contracts.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent.Validators
{
    public class GetChildrenUsersQueryValidator : AbstractValidator<GetChildrenUsersQuery>
    {
        public GetChildrenUsersQueryValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;   // 不同字段之间：第一个字段挂了就不再检查后续字段
            RuleLevelCascadeMode = CascadeMode.Stop;   // 同一字段内：第一条规则挂了就不再检查该字段的后续规则

            RuleFor(x => x.AgentUserid)
                .GreaterThan(0).WithMessage("代理id必须大于0");

            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(1).WithMessage("页码必须大于等于 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("每页条数需在 1-100 之间");

            RuleFor(x => x.Keyword)
                .MaximumLength(50).WithMessage("关键词长度不能超过 50 字符")
                .When(x => !string.IsNullOrWhiteSpace(x.Keyword));
        }
    }
}
