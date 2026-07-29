using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utilities
{
    public static class EnumerableExtension
    {
        public static bool SafeAny<T>(this IEnumerable<T> values) => values != null && values.Any();
    }
}
