using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class AuthorizationException : ExceptionWithErrorCode
    {
        public AuthorizationException() : base(string.Empty, ErrorCodes.Forbidden)
        {
        }

        public AuthorizationException(string message) : base(message, ErrorCodes.Forbidden)
        {
        }

        public AuthorizationException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}