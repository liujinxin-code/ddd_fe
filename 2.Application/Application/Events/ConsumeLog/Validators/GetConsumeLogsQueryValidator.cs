using Application.Events.ConsumeLogs.Contracts.Queries;
using FluentValidation;

namespace Application.Events.ConsumeLogs.Validators
{
    public class GetConsumeLogsQueryValidator : AbstractValidator<GetConsumeLogsQuery>
    {
        public GetConsumeLogsQueryValidator()
        {
            // -1 表示不过滤类型；其余仅允许 0-6 七个合法类型值（含新增的 AgentWithdrawOut=6）。
            RuleFor(x => x.ConsumeStatus)
                .InclusiveBetween(-1, 6).WithMessage("消费类型取值须在 -1(全部) 到 6 之间");

            RuleFor(x => x.Keyword)
                .MaximumLength(100).WithMessage("检索关键字长度不能超过100")
                .When(x => !string.IsNullOrEmpty(x.Keyword));

            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("页码必须大于0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("每页条数须在 1-100 之间");
        }
    }
}
