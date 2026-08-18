using Application.Abstractions;
using Application.Common.Models;
using Application.Abstractions.Repositories;

using Application.Features.ServiceImage;
using MediatR;
using Shared.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ServiceImage
{
    public class UploadAgentWechatImageCommandHandler(
            ITkUserRepository tkUserRepository,
            IServiceImageRepository serviceImageRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
        ) : IRequestHandler<UploadAgentWechatImageCommand, Unit>
    {
        public async Task<Unit> Handle(UploadAgentWechatImageCommand cmd, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
                throw new UnauthorizedDomainException();

            var user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null) throw new BusinessException("用户不存在");
            if (!user.IsAgentFnc()) throw new BusinessException("非代理用户无法上传客服图片");

            // 原子 upsert：首次插入、后续仅更新 URL，并发首次上传不会触发唯一键冲突。
            await serviceImageRepository.UpsertByAgentUserAsync(currentUser.Userid, cmd.ImageUrl, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
