using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class BadRequestException : ExceptionWithErrorCode
    {
        public BadRequestException() : base(string.Empty, ErrorCodes.BadRequest)
        {
        }

        public BadRequestException(string message) : base(message, ErrorCodes.BadRequest)
        {
        }

        public BadRequestException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}