using Application.Features.Order;
using FluentValidation;

namespace Application.Features.Order
{
    public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
    {
        public GetOrdersQueryValidator()
        {
            // 0 表示不过滤状态；其余仅允许 1-4 四个合法状态值。
            RuleFor(x => x.OrderState)
                .InclusiveBetween(0, 4).WithMessage("订单状态取值须在 0-4 之间（0 为不筛选）");

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
