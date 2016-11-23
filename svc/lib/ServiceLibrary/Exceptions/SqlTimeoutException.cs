using System;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class SqlTimeoutException : ExceptionWithErrorCode
    {
        public SqlTimeoutException()
        {
        }

        public SqlTimeoutException(string message) : base(message)
        {
        }

        public SqlTimeoutException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
