using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.Caching
{
    public class RedisOptions
    {
        public const string SectionName = "RedisOptions";

        public string ConnectionString { get; set; } = default!;
    }
}
