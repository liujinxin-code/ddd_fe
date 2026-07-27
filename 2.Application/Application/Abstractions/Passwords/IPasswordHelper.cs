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
        /// 生成8位数随机密码
        /// </summary>
        /// <returns></returns>
        string GenerateRandomPwd();
    }
}
