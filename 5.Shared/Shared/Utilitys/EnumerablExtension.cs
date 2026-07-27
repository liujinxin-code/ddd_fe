using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utilitys
{
    public static class EnumerablExtension
    {
        public static bool SafeAny<T>(this IEnumerable<T> values) => values != null && values.Any();
    }
}
