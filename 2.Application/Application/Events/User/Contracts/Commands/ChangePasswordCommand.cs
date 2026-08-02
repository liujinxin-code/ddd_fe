using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.User.Contracts.Commands
{
    /// <summary>
    /// 修改当前登录用户密码：需提供原密码校验，成功后更新为新密码哈希。
    /// UserId 由 Controller 从当前登录用户注入，避免越权。
    /// </summary>
    public record ChangePasswordCommand(string OldPassword, string NewPassword, long UserId = 0)
        : IRequest<ApiResult>;
}
