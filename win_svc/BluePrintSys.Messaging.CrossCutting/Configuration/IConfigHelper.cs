using BluePrintSys.Messaging.CrossCutting.Models.Enums;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.CrossCutting.Configuration
{
    public interface IConfigHelper
    {
        string NServiceBusConnectionString { get; }

        MessageBroker GetMessageBroker();

        string MessageQueue { get; }

        string ErrorQueue { get; }

        int MessageProcessingMaxConcurrency { get; }

        MessageActionType SupportedActionTypes { get; }

        Tenancy Tenancy { get; }
        int CacheExpirationMinutes { get; }
        string SingleTenancyConnectionString { get; }
        string NServiceBusInstanceId { get; }
    }
}
