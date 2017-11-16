using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class RabbitMqSendException : ExceptionWithErrorCode
    {
        public RabbitMqSendException(Exception ex) : base("The message could not be sent because RabbitMQ could not be reached. Please check the NServiceBus.ConnectionString settings. " + ex.Message, ErrorCodes.RabbitMqSend)
        {
        }
    }
}
