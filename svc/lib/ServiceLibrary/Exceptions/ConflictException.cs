using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class ConflictException : ExceptionWithErrorCode
    {
        public ConflictException() : this(string.Empty)
        {
        }

        public ConflictException(string message) : this(message, ErrorCodes.Conflict)
        {
        }

        public ConflictException(string message, int errorCode) : this(message, errorCode, null)
        {
        }

        public ConflictException(string message, int errorCode, object content) : base(message, errorCode, content)
        {
        }
    }
}
