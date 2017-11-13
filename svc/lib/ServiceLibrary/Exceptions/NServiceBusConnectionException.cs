using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class NServiceBusConnectionException : ExceptionWithErrorCode
    {
        public NServiceBusConnectionException(string connectionString) : base("The NServiceBus connection string is invalid: " + connectionString, ErrorCodes.NServiceBusConnection)
        {
        }
    }
}
