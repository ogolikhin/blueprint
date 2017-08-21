using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;

namespace AdminStore.Services.Email
{
    [Serializable]
    public class EmailException : ExceptionWithErrorCode
    {
        public EmailException() { }
        public EmailException(string message) : base(message) { }
        public EmailException(string message, int errorCode) : base(message, errorCode) { }
    }
}
