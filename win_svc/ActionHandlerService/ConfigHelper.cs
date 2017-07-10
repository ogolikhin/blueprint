using BluePrintSys.Messaging.CrossCutting;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService
{
    public enum Tenancy
    {
        Single,
        Multiple
    }

    public enum Transport
    {
        RabbitMQ,
        SQL
    }

    public static class ConfigHelper
    {
        private const string ServiceNameKey = "Service.Name";
        private const string ServiceNameDefault = "BlueprintActionHandler";
        public static string ServiceName => AppSettingsHelper.GetConfigStringValue(ServiceNameKey, ServiceNameDefault);

        private const string MessageProcessingMaxConcurrencyKey = "MessageProcessing.MaxConcurrency";
        private const int MessageProcessingMaxConcurrencyDefault = 1;
        public static int MessageProcessingMaxConcurrency => AppSettingsHelper.GetConfigIntValue(MessageProcessingMaxConcurrencyKey, MessageProcessingMaxConcurrencyDefault);

        private const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        private const string NServiceBusConnectionStringDefault = "host=titan.blueprintsys.net;username=admin;password=$admin2011";
        public static string NServiceBusConnectionString => AppSettingsHelper.GetConfigStringValue(NServiceBusConnectionStringKey, NServiceBusConnectionStringDefault);

        private const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        private const string NServiceBusInstanceIdDefault = "";
        public static string NServiceBusInstanceId => AppSettingsHelper.GetConfigStringValue(NServiceBusInstanceIdKey, NServiceBusInstanceIdDefault);

        private const string SingleTenancyConnectionStringKey = "SingleTenancyConnectionString";
        private const string SingleTenancyConnectionStringDefault = "";
        public static string SingleTenancyConnectionString => AppSettingsHelper.GetConfigStringValue(SingleTenancyConnectionStringKey, SingleTenancyConnectionStringDefault);

        private const string CacheExpirationMinutesKey = "CacheExpirationMinutes";
        private const int CacheExpirationMinutesDefault = 1440;
        public static int CacheExpirationMinutes => AppSettingsHelper.GetConfigIntValue(CacheExpirationMinutesKey, CacheExpirationMinutesDefault);

        private const string TenancyKey = nameof(Tenancy);
        private const Tenancy TenancyDefault = Tenancy.Single;
        public static Tenancy Tenancy => AppSettingsHelper.GetConfigEnum(TenancyKey, TenancyDefault);

        private const string TransportKey = nameof(Transport);
        private const Transport TransportDefault = Transport.RabbitMQ;
        public static Transport Transport => AppSettingsHelper.GetConfigEnum(TransportKey, TransportDefault);

        private const string ActionTypesKey = "ActionTypesCsv";
        private const MessageActionType ActionTypesDefault = MessageActionType.All;
        public static MessageActionType AllowedActionTypes => AppSettingsHelper.GetConfigEnum(ActionTypesKey, ActionTypesDefault);

        private const string HandlerKey = "Handler";
        private const string HandlerKeyDefault = "Cloud.MessageServer";
        public static string Handler => AppSettingsHelper.GetConfigStringValue(HandlerKey, HandlerKeyDefault);

        private const string ErrorQueueKey = "ErrorQueue";
        private const string ErrorQueueDefault = "errors";
        public static string ErrorQueue => AppSettingsHelper.GetConfigStringValue(ErrorQueueKey, ErrorQueueDefault);
    }
}
