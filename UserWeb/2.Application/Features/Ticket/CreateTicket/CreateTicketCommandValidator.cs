using Application.Features.Ticket;
using FluentValidation;

namespace Application.Features.Ticket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.TicketContent)
            .NotEmpty().WithMessage("请填写工单内容")
            .MaximumLength(3000).WithMessage("工单内容不能超过 3000 字");

        RuleFor(x => x.TicketType)
            .InclusiveBetween(0, 3).WithMessage("问题类型无效");

        RuleFor(x => x.TicketImages)
            .Must(images => images == null || images.Count <= 5)
            .WithMessage("最多上传 5 张图片")
            .ForEach(img => img.MaximumLength(2000).WithMessage("图片地址过长"));
    }
}
