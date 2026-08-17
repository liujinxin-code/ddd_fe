using Application.Abstractions;
using Application.Abstractions.Passwords;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Features.User;
using Domain.Entities;
using MediatR;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.User
{
    public class ChangePasswordCommandHandler(
        ITkUserRepository tkUserRepository,
        IUnitOfWork unitOfWork,
        IPasswordHelper passwordHelper,
        ICurrentUser currentUser) : IRequestHandler<ChangePasswordCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(ChangePasswordCommand cmd, CancellationToken ct)
        {
            if (currentUser.Userid <= 0)
            {
                throw new BusinessException("用户未登录或身份无效");
            }
            if (string.IsNullOrWhiteSpace(cmd.OldPassword))
            {
                throw new BusinessException("请输入原密码");
            }
            if (string.IsNullOrWhiteSpace(cmd.NewPassword))
            {
                throw new BusinessException("请输入新密码");
            }
            if (cmd.NewPassword.Trim().Length < 6)
            {
                throw new BusinessException("新密码长度至少 6 位");
            }

            // 取被追踪实体，更新后由 IUnitOfWork 持久化
            var user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null)
            {
                throw new BusinessException("用户不存在");
            }

            // 校验原密码（Argon2id 常时比较在基础设施层完成）
            if (!passwordHelper.VerifyPassword(cmd.OldPassword, user.Password))
            {
                throw new BusinessException("原密码不正确");
            }

            // 新旧密码不得相同（防范无意义修改，非强制业务要求，可按需移除）
            if (passwordHelper.VerifyPassword(cmd.NewPassword, user.Password))
            {
                throw new BusinessException("新密码不能与原密码相同");
            }

            user.ChangePasswordFunc(passwordHelper.GeneratePasswordHash(cmd.NewPassword));
            await unitOfWork.SaveChangesAsync(ct);

            return ApiResult.Successed("密码修改成功");
        }
    }
}
