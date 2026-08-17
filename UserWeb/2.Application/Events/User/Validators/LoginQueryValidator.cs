using Application.Events.User.Contracts.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.User.Validators
{

    public class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;   // 不同字段之间：第一个字段挂了就不再检查后续字段
            RuleLevelCascadeMode = CascadeMode.Stop;   // 同一字段内：第一条规则挂了就不再检查该字段的后续规则

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("请传入用户名或邮箱号");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("密码不能为空");
        }
    }
}
