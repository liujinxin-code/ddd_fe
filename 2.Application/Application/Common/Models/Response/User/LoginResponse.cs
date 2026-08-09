namespace Application.Common.Models.Response.User
{
    public record LoginResponse
    {
        /// <summary>
        /// 登录令牌
        /// </summary>
        public string Token { get; set; } = default!;
        /// <summary>
        /// 登录用户信息
        /// </summary>
        public LoginUserResponse User { get; set; } = default!;

    }
    public record LoginUserResponse
    {
        public long Userid { get; set; }

        public string Username { get; set; } = default!;

        public decimal AgentAmount { get; set; }
    }
}
