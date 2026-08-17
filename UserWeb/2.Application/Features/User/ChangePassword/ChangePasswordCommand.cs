using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User
{
    /// <summary>
    /// 修改当前登录用户密码：需提供原密码校验，成功后更新为新密码哈希。
    /// 当前登录用户id 由 ICurrentUser 注入，避免越权。
    /// </summary>
    public record ChangePasswordCommand(string OldPassword, string NewPassword)
        : IRequest<ApiResult>;
}
