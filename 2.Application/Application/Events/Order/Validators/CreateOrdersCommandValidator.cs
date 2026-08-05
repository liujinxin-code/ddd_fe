using Application.Common.Models.Order;
using Application.Events.Order.Contracts.Commands;
using FluentValidation;
using System.Collections.Generic;
using System.Linq;

namespace Application.Events.Order.Validators
{
    /// <summary>
    /// 批量下单基础校验（与价格/余额/业务模板相关的强校验在 Handler 内完成，因其需查库）。
    /// 校验失败由全局 ValidationBehavior 抛出 ValidationException，统一转 400。
    /// </summary>
    public class CreateOrdersCommandValidator : AbstractValidator<CreateOrdersCommand>
    {
        /// <summary>单条明细允许携带的评论条数上限，防滥用。</summary>
        private const int MaxCommentsPerItem = 1000;

        public CreateOrdersCommandValidator()
        {
            RuleFor(x => x.Items)
                .NotNull().WithMessage("订单明细不能为空")
                .Must(items => items.Count > 0).WithMessage("订单明细不能为空")
                .Must(items => items.Count <= 100).WithMessage("单次最多提交 100 条订单");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ConfigId)
                    .GreaterThan(0).WithMessage("业务配置id必须大于 0");

                item.RuleFor(i => i.Quantity)
                    .GreaterThanOrEqualTo(0).WithMessage("订单数量不能为负数");

                item.RuleFor(i => i.OrderLink)
                    .MaximumLength(500).WithMessage("下单链接不能超过 500 字符");

                // 数量与评论内容至少提供其一：粉丝/账户业务给 Quantity，评论业务给 Comments（评论条数即数量）。
                item.RuleFor(i => i)
                    .Must(i => i.Quantity > 0 || (i.Comments != null && i.Comments.Any(c => !string.IsNullOrWhiteSpace(c))))
                    .WithMessage("订单数量必须大于 0（评论业务无需传数量，提交评论内容即可，评论条数即数量）");

                item.RuleFor(i => i.Comments)
                    .Must(cs => cs == null || cs.Count <= MaxCommentsPerItem)
                    .WithMessage($"单条明细最多提交 {MaxCommentsPerItem} 条评论")
                    .Must(cs => cs == null || cs.All(c => (c ?? string.Empty).Trim().Length <= 500))
                    .WithMessage("单条评论内容不能超过 500 字符");
            });
        }
    }
}
