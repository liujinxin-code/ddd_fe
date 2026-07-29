using Application.Abstractions.Passwords;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.Passwords
{
    public class PasswordHelper(IOptions<PasswordOptions> options) : IPasswordHelper
    {
        public string GeneratePasswordHash(string password)
        {
            var salt = Encoding.UTF8.GetBytes(options.Value.Salt);
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 4,   // ✅ 4 核
                MemorySize = 65536,        // ✅ 64 MB
                Iterations = 4
            };

            var hash = argon2.GetBytes(32);
            return Convert.ToBase64String(salt.Concat(hash).ToArray());
        }

        public string GenerateRandomPwd()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";

            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();

            // 生成 4 位英文
            var letters = new char[4];
            for (int i = 0; i < 4; i++)
            {
                letters[i] = chars[GetRandomIndex(rng, chars.Length)];
            }

            // 生成 4 位数字
            var numbers = new char[4];
            for (int i = 0; i < 4; i++)
            {
                numbers[i] = digits[GetRandomIndex(rng, digits.Length)];
            }

            return new string(letters) + new string(numbers);
        }
        private static int GetRandomIndex(System.Security.Cryptography.RandomNumberGenerator rng, int max)
        {
            byte[] buffer = new byte[4];
            rng.GetBytes(buffer);
            return Math.Abs(BitConverter.ToInt32(buffer, 0)) % max;
        }
    }



}
