using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.CrossCutting.Configuration
{
    public interface IConfigHelper
    {
        string NServiceBusConnectionString { get; }

        int NServiceBusSendTimeoutSeconds { get; }

        string MessageQueue { get; }

        string ErrorQueue { get; }

        int MessageProcessingMaxConcurrency { get; }

        MessageActionType SupportedActionTypes { get; }

        int CacheExpirationMinutes { get; }

        string TenantsDatabase { get; }

        string NServiceBusInstanceId { get; }

        int WebhookConnectionTimeout { get; }

        int WebhookRetryCount { get; }

        int WebhookRetryInterval { get; }
    }
}
