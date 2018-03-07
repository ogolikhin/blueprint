using ServiceLibrary.Models.Enums;
using System;

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

        TimeSpan NServiceBusCriticalErrorRetryDelay { get; }
        int NServiceBusCriticalErrorRetryCount { get; }
        bool NServiceBusIgnoreCriticalErrors { get; }

        int NServiceBusNumberOfImmediateRetries { get; }
        int NServiceBusNumberOfDelayedRetries { get; }
        TimeSpan NServiceBusDelayIntervalIncrease { get; }
    }
}
