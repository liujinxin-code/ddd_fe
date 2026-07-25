using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
    public class CurrentUser
    {
        public long Userid { get; set; }

        public string Username { get; set; }

        public string Jti { get; set; }
    }
}
