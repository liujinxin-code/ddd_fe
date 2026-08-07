using Application.Events.ServiceImage.Contracts.Commands;
using FluentValidation;

namespace Application.Events.ServiceImage.Validators
{
    public class UploadAgentWechatImageCommandValidator : AbstractValidator<UploadAgentWechatImageCommand>
    {
        public UploadAgentWechatImageCommandValidator()
        {
            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("图片地址不能为空")
                .MaximumLength(500).WithMessage("图片地址长度不能超过500");
        }
    }
}
