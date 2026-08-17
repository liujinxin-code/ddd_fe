using Application.Events.User.Contracts.Commands;
using Application.Events.User.Contracts.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.User.Validators
{
    /// <summary>
    /// CreateUserCommand 的参数校验（校验「命令」而非「实体」）
    /// </summary>
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;   // 不同字段之间：第一个字段挂了就不再检查后续字段
            RuleLevelCascadeMode = CascadeMode.Stop;   // 同一字段内：第一条规则挂了就不再检查该字段的后续规则

            RuleFor(x => x.username)
         .NotEmpty().WithMessage("用户名不能为空")
         .MaximumLength(50).WithMessage("用户名长度不能超过 50 字符")
          .Matches(@"^[A-Za-z0-9]+$")
 .WithMessage("用户名只能包含英文字母和数字");

            RuleFor(x => x.email)
                .NotEmpty().WithMessage("邮箱不能为空")
                .EmailAddress().WithMessage("邮箱格式不正确")
                .MaximumLength(100).WithMessage("邮箱长度不能超过 100 字符");

            RuleFor(x => x.password)
                .NotEmpty().WithMessage("密码不能为空")
                .MinimumLength(8).WithMessage("密码至少 8 位")
                   .MaximumLength(25).WithMessage("密码不能大于 25 位")
                .Matches("[A-Z]").WithMessage("密码需包含大写字母")
                .Matches("[0-9]").WithMessage("密码需包含数字");
        }
    }


}
