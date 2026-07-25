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

    }
}
