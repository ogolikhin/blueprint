using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class InvalidConnectionStringException : ExceptionWithErrorCode
    {
        public InvalidConnectionStringException(string message) : base(message, ErrorCodes.InvalidConnectionString)
        {
        }
    }
}
