using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class InvalidColumnsException : ExceptionWithErrorCode
    {
        public InvalidColumnsException(object items) : base(string.Empty, ErrorCodes.BadRequest, items)
        {

        }
    }
}
