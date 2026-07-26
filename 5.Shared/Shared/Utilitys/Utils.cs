using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utilitys
{
    public class Utils
    {
        public static string GetStringToHash(string value)
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            string hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();
            return hashString;
        }

        /// <summary>
        /// 生成唯一序列号
        /// </summary>
        /// <returns></returns>
        public static string GenerateSerialNo(int length = 24, string serailNoPre = "")
        {
            int no = 22;
            if (length < 24)
            {
                throw new Exception("请输入24或以上的序列号长度");
            }
            string value = "";
            int count = length - no;
            for (int i = 0; i < count; i++)
            {
                value = value + (char)new Random(Guid.NewGuid().GetHashCode()).Next(65, 91);
            }
            var orderNo = $"{serailNoPre}{DateTime.Now:yyMMddHHmmssfff}{value}{new Random(Guid.NewGuid().GetHashCode()).Next(1000000, 9999999)}";
            return orderNo;
        }
    }
}
