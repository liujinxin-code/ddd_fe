using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.Passwords
{
    public class PasswordOptions
    {
        public const string SectionName = "PasswordOptions";

        public string Salt { get; set; } = default!;
    }
}
