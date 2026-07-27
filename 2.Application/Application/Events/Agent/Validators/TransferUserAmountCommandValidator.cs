using Application.Events.Agent.Contracts.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent.Validators
{


    public class TransferUserAmountCommandValidator : AbstractValidator<TransferUserAmountCommand>
    {
        public TransferUserAmountCommandValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;   // 不同字段之间：第一个字段挂了就不再检查后续字段
            RuleLevelCascadeMode = CascadeMode.Stop;   // 同一字段内：第一条规则挂了就不再检查该字段的后续规则

            RuleFor(x => x.ChildrenUserid)
                .NotEmpty().WithMessage("下级用户id 不能为空")
                .GreaterThan(0).WithMessage("用户id必须大于0");

            RuleFor(x => x.transferAmount)
        .GreaterThan(0).WithMessage("转赠金额必须大于0");


        }

    }
}
