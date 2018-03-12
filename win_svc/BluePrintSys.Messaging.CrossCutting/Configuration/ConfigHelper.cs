using ServiceLibrary.Models.Enums;
using System;

namespace BluePrintSys.Messaging.CrossCutting.Configuration
{
    public class ConfigHelper : IConfigHelper
    {
        public const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        public const string NServiceBusConnectionStringDefault = "";
        public string NServiceBusConnectionString => AppSettingsHelper.GetConfigStringValue(NServiceBusConnectionStringKey, NServiceBusConnectionStringDefault);

        public const string NServiceBusSendTimeoutSecondsKey = "NServiceBus.SendTimeoutSeconds";
        public const int DefaultNServiceBusSendTimeoutSeconds = 60;
        public int NServiceBusSendTimeoutSeconds => AppSettingsHelper.GetConfigIntValue(NServiceBusSendTimeoutSecondsKey, DefaultNServiceBusSendTimeoutSeconds);

        public const string MessageQueueKey = "NServiceBus.Messaging.MessageQueue";
        public const string MessageQueueDefault = "Blueprint.Workflow";
        public string MessageQueue => AppSettingsHelper.GetConfigStringValue(MessageQueueKey, MessageQueueDefault);

        public const string ErrorQueueKey = "NServiceBus.Messaging.ErrorQueue";
        public const string ErrorQueueDefault = "errors";
        public string ErrorQueue => AppSettingsHelper.GetConfigStringValue(ErrorQueueKey, ErrorQueueDefault);

        public const string MessageProcessingMaxConcurrencyKey = "MessageProcessing.MaxConcurrency";
        public const int MessageProcessingMaxConcurrencyDefault = 1;
        public int MessageProcessingMaxConcurrency => AppSettingsHelper.GetConfigIntValue(MessageProcessingMaxConcurrencyKey, MessageProcessingMaxConcurrencyDefault);

        public const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        public const string NServiceBusInstanceIdDefault = "";
        public string NServiceBusInstanceId => AppSettingsHelper.GetConfigStringValue(NServiceBusInstanceIdKey, NServiceBusInstanceIdDefault);

        public const string TenantsDatabaseKey = "TenantsDatabase";
        public string TenantsDatabase => AppSettingsHelper.GetConnectionStringValue(TenantsDatabaseKey);

        public const string CacheExpirationMinutesKey = "CacheExpirationMinutes";
        public const int CacheExpirationMinutesDefault = 10080;
        public int CacheExpirationMinutes => AppSettingsHelper.GetConfigIntValue(CacheExpirationMinutesKey, CacheExpirationMinutesDefault);

        public const string SupportedActionTypesKey = "SupportedActionTypes";
        public const MessageActionType SupportedActionTypesDefault = MessageActionType.All;
        public MessageActionType SupportedActionTypes => AppSettingsHelper.GetConfigEnum(SupportedActionTypesKey, SupportedActionTypesDefault);

        // Webhooks
        public const string WebhookConnectionTimeoutKey = "WebhookConnectionTimeout";
        public const int WebhookConnectionTimeoutDefault = 20;
        public int WebhookConnectionTimeout => AppSettingsHelper.GetConfigIntValue(WebhookConnectionTimeoutKey, WebhookConnectionTimeoutDefault);

        public const string WebhookRetryCountKey = "WebhookRetryCount";
        public const int WebhookRetryCountDefault = 5;
        public int WebhookRetryCount => AppSettingsHelper.GetConfigIntValue(WebhookRetryCountKey, WebhookRetryCountDefault);

        public const string WebhookRetryIntervalKey = "WebhookRetryInterval";
        public const int WebhookRetryIntervalDefault = 900;
        public int WebhookRetryInterval => AppSettingsHelper.GetConfigIntValue(WebhookRetryIntervalKey, WebhookRetryIntervalDefault);

        // NSB - Critical Error handling
        // <!-- Delay before retry in minutes -->
        // <add key = "NServiceBus.CriticalErrorRetryDelay" value="2" />
        public const string NServiceBusCriticalErrorRetryDelayKey = "NServiceBus.CriticalErrorRetryDelay";
        public const int NServiceBusCriticalErrorRetryDelayDefault = 2;
        public TimeSpan NServiceBusCriticalErrorRetryDelay =>
            TimeSpan.FromMinutes(AppSettingsHelper.GetConfigIntValue(NServiceBusCriticalErrorRetryDelayKey, NServiceBusCriticalErrorRetryDelayDefault));

        // <!-- Number of unsuccesseful retries before service restart/stop -->
        // <add key = "NServiceBus.CriticalErrorRetryCount" value ="3"/>
        public const string NServiceBusCriticalErrorRetryCountKey = "NServiceBus.CriticalErrorRetryCount";
        public const int NServiceBusCriticalErrorRetryCountDefault = 3;
        public int NServiceBusCriticalErrorRetryCount =>
            AppSettingsHelper.GetConfigIntValue(NServiceBusCriticalErrorRetryCountKey, NServiceBusCriticalErrorRetryCountDefault);

        public const string NServiceBusIgnoreCriticalErrorsKey = "NServiceBus.IgnoreCriticalErrors";
        public const bool NServiceBusIgnoreCriticalErrorsDefault = false;
        public bool NServiceBusIgnoreCriticalErrors =>
            AppSettingsHelper.GetConfigBoolValue(NServiceBusIgnoreCriticalErrorsKey, NServiceBusIgnoreCriticalErrorsDefault);

        // Default Retry Policy configuration
        public const string NServiceBusNumberOfImmediateRetriesKey = "NServiceBus.NumberOfImmediateRetries";
        public const int NServiceBusNumberOfImmediateRetriesDefault = 3;
        public int NServiceBusNumberOfImmediateRetries =>
            AppSettingsHelper.GetConfigIntValue(NServiceBusNumberOfImmediateRetriesKey, NServiceBusNumberOfImmediateRetriesDefault);

        public const string NServiceBusNumberOfDelayedRetriesKey = "NServiceBus.NumberOfDelayedRetries";
        public const int NServiceBusNumberOfDelayedRetriesDefault = 5;
        public int NServiceBusNumberOfDelayedRetries =>
            AppSettingsHelper.GetConfigIntValue(NServiceBusNumberOfDelayedRetriesKey, NServiceBusNumberOfDelayedRetriesDefault);

        public const string NServiceBusDelayIntervalIncreaseKey = "NServiceBus.DelayIntervalIncrease";
        public const int NServiceBusDelayIntervalIncreaseDefault = 10;
        public TimeSpan NServiceBusDelayIntervalIncrease =>
            TimeSpan.FromMinutes(AppSettingsHelper.GetConfigIntValue(NServiceBusDelayIntervalIncreaseKey, NServiceBusDelayIntervalIncreaseDefault));
    }
}
