using Application.Abstractions.Passwords;
using Microsoft.Extensions.Options;
using Shared.Utilitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.Passwords
{
    public class PasswordHelper(IOptions<PasswordOptions> options) : IPasswordHelper
    {
        public string GeneratePasswordHash(string password) => Utils.GetStringToHash(password + options.Value.Salt);
    }
}
