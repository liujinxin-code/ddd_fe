using Application.Events.Ticket.Contracts.Queries;
using FluentValidation;

namespace Application.Events.Ticket.Validators;

public class GetTicketsQueryValidator : AbstractValidator<GetTicketsQuery>
{
    public GetTicketsQueryValidator()
    {
        RuleFor(x => x.TicketStatus)
            .InclusiveBetween(-1, 1).WithMessage("工单状态筛选无效");

        RuleFor(x => x.TicketType)
            .InclusiveBetween(-1, 3).WithMessage("问题类型筛选无效");

        RuleFor(x => x.Keyword)
            .MaximumLength(100).WithMessage("关键词过长");

        RuleFor(x => x.PageIndex)
            .GreaterThan(0).WithMessage("页码必须大于 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("每页条数需在 1-100 之间");
    }
}
