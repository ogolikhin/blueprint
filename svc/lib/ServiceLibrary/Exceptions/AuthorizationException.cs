using System;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class AuthorizationException : ExceptionWithErrorCode
    {
        public AuthorizationException() : base()
        {
        }

        public AuthorizationException(string message) : base(message)
        {
        }

        public AuthorizationException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}