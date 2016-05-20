using System;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class AuthenticationException : ExceptionWithErrorCode
    {
        public AuthenticationException(string message) : base(message)
        {
        }

        public AuthenticationException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}