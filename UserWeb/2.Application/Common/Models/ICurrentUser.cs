namespace Application.Common.Models
{
    /// <summary>
    /// 当前登录用户抽象。由 Infrastructure 层的 CurrentUserAccessor 基于 JWT Claims 实现，
    /// 以 Scoped 生命周期注入 Application 层的 Command/Query Handler 构造函数。
    /// 用户身份只来源于经过 [Authorize] 验证的 JWT，前台无法伪造。
    /// </summary>
    public interface ICurrentUser
    {
        long Userid { get; }
        string Username { get; }
        string Jti { get; }
        bool IsAuthenticated { get; }
    }


}
