using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Passwords
{
    public interface IPasswordHelper
    {
        string GeneratePasswordHash(string password);
        /// <summary>
        /// 验证明文密码与存储的哈希是否匹配（Argon2id，固定盐，常时比较）。
        /// </summary>
        bool VerifyPassword(string password, string storedHash);
        /// <summary>
        /// 生成8位数随机密码
        /// </summary>
        /// <returns></returns>
        string GenerateRandomPwd();
    }
}
