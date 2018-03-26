using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class NServiceBusSendTimeoutException : ExceptionWithErrorCode
    {
        public NServiceBusSendTimeoutException(string message) : base("NServiceBus message failed to send due to a timeout. " + message, ErrorCodes.NServiceBusSendTimeout)
        {
        }
    }
}
