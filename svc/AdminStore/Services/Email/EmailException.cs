using System;
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
