using System;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class ConflictException : ExceptionWithErrorCode
    {
        public ConflictException() : base()
        {
        }

        public ConflictException(string message) : base(message)
        {
        }

        public ConflictException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
