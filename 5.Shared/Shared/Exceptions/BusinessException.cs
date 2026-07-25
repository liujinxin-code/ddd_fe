using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Exceptions
{
    public class BusinessException : Exception
    {
        public int ErrorCode { get; }

        public BusinessException(string message, int errorCode = 400)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
