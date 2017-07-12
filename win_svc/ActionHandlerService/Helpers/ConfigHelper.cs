using ActionHandlerService.Models.Enums;
using BluePrintSys.Messaging.CrossCutting;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.Helpers
{
    public static class ConfigHelper
    {
        private const string ServiceNameKey = "Service.Name";
        private const string ServiceNameDefault = "BlueprintActionHandler";
        public static string ServiceName => AppSettingsHelper.GetConfigStringValue(ServiceNameKey, ServiceNameDefault);

        private const string MessageProcessingMaxConcurrencyKey = "MessageProcessing.MaxConcurrency";
        private const int MessageProcessingMaxConcurrencyDefault = 1;
        public static int MessageProcessingMaxConcurrency => AppSettingsHelper.GetConfigIntValue(MessageProcessingMaxConcurrencyKey, MessageProcessingMaxConcurrencyDefault);

        private const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        private const string NServiceBusConnectionStringDefault = "";
        public static string NServiceBusConnectionString => AppSettingsHelper.GetConfigStringValue(NServiceBusConnectionStringKey, NServiceBusConnectionStringDefault);

        private const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        private const string NServiceBusInstanceIdDefault = "";
        public static string NServiceBusInstanceId => AppSettingsHelper.GetConfigStringValue(NServiceBusInstanceIdKey, NServiceBusInstanceIdDefault);

        private const string SingleTenancyConnectionStringKey = "TenantsDatabase";
        public static string SingleTenancyConnectionString => AppSettingsHelper.GetConnectionStringValue(SingleTenancyConnectionStringKey);

        private const string CacheExpirationMinutesKey = "CacheExpirationMinutes";
        private const int CacheExpirationMinutesDefault = 1440;
        public static int CacheExpirationMinutes => AppSettingsHelper.GetConfigIntValue(CacheExpirationMinutesKey, CacheExpirationMinutesDefault);

        private const string TenancyKey = nameof(Tenancy);
        private const Tenancy TenancyDefault = Tenancy.Single;
        public static Tenancy Tenancy => AppSettingsHelper.GetConfigEnum(TenancyKey, TenancyDefault);

        private const string TransportKey = nameof(Transport);
        private const MessageTransport TransportDefault = MessageTransport.RabbitMQ;
        public static MessageTransport Transport => AppSettingsHelper.GetConfigEnum(TransportKey, TransportDefault);

        private const string SupportedActionTypesKey = "SupportedActionTypes";
        private const MessageActionType SupportedActionTypesDefault = MessageActionType.All;
        public static MessageActionType SupportedActionTypes => AppSettingsHelper.GetConfigEnum(SupportedActionTypesKey, SupportedActionTypesDefault);

        private const string MessageQueueKey = "MessageQueue";
        private const string MessageQueueKeyDefault = "Cloud.MessageServer";
        public static string MessageQueue => AppSettingsHelper.GetConfigStringValue(MessageQueueKey, MessageQueueKeyDefault);

        private const string ErrorQueueKey = "ErrorQueue";
        private const string ErrorQueueDefault = "errors";
        public static string ErrorQueue => AppSettingsHelper.GetConfigStringValue(ErrorQueueKey, ErrorQueueDefault);
    }
}
