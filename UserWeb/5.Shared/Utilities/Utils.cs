using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utilities
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
        public static string GenerateSerialNo(int length = 24, string serialNoPre = "")
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
            var orderNo = $"{serialNoPre}{DateTimeOffset.Now:yyMMddHHmmssfff}{value}{new Random(Guid.NewGuid().GetHashCode()).Next(1000000, 9999999)}";
            return orderNo;
        }

        /// <summary>
        /// 金额四舍五入保留 6 位小数（半进位，MidpointRounding.AwayFromZero），与 decimal(10,6) 列精度一致。
        /// </summary>
        public static decimal RoundToSixDecimals(decimal value)
        {
            return Math.Round(value, 6, MidpointRounding.AwayFromZero);
        }
    }
}
