using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class DbOptions
    {
        public const string SectionName = "DbOptions";
        public string ConnectionString { get; set; } = default!;
    }
}
