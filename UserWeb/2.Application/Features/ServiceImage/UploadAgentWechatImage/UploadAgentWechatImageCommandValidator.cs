using Application.Features.ServiceImage;
using FluentValidation;

namespace Application.Features.ServiceImage
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
