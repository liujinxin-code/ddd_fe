using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Exceptions
{
    public class ConcurrencyConflictException : Exception
    {
        public int ErrorCode { get; }

        public ConcurrencyConflictException(string message, Exception ex, int errorCode = 400)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
