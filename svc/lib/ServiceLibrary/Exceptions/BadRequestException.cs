using System;
using System.Runtime.Serialization;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class BadRequestException : ExceptionWithErrorCode
    {
        public BadRequestException() : base()
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}