using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class ConflictException : ExceptionWithErrorCode
    {
        public ConflictException() : this("")
        {
        }

        public ConflictException(string message) : this(message, ErrorCodes.Conflict)
        {
        }

        public ConflictException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
