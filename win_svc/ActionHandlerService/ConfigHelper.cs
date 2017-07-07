using BluePrintSys.Messaging.CrossCutting;
using BluePrintSys.Messaging.Models.Action;

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
        const string ServiceNameKey = "Service.Name";
         const string ServiceNameDefault = "BlueprintActionHandler";
        public static string ServiceName => AppSettingsHelper.GetConfigStringValue(ServiceNameKey, ServiceNameDefault);

        const string MessageProcessingMaxConcurrencyKey = "MessageProcessing.MaxConcurrency";
        const int MessageProcessingMaxConcurrencyDefault = 1;
        public static int MessageProcessingMaxConcurrency => AppSettingsHelper.GetConfigIntValue(MessageProcessingMaxConcurrencyKey, MessageProcessingMaxConcurrencyDefault);

        const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        const string NServiceBusConnectionStringDefault = "host=titan.blueprintsys.net;username=admin;password=$admin2011";
        public static string NServiceBusConnectionString => AppSettingsHelper.GetConfigStringValue(NServiceBusConnectionStringKey, NServiceBusConnectionStringDefault);

        const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        const string NServiceBusInstanceIdDefault = "";
        public static string NServiceBusInstanceId => AppSettingsHelper.GetConfigStringValue(NServiceBusInstanceIdKey, NServiceBusInstanceIdDefault);

        const string SingleTenancyConnectionStringKey = "SingleTenancyConnectionString";
        const string SingleTenancyConnectionStringDefault = "";
        public static string SingleTenancyConnectionString => AppSettingsHelper.GetConfigStringValue(SingleTenancyConnectionStringKey, SingleTenancyConnectionStringDefault);

        const string CacheExpirationMinutesKey = "CacheExpirationMinutes";
        const int CacheExpirationMinutesDefault = 1440;
        public static int CacheExpirationMinutes => AppSettingsHelper.GetConfigIntValue(CacheExpirationMinutesKey, CacheExpirationMinutesDefault);

        const string TenancyKey = nameof(Tenancy);
        const Tenancy TenancyDefault = Tenancy.Single;
        public static Tenancy Tenancy => AppSettingsHelper.GetConfigEnum(TenancyKey, TenancyDefault);

        const string TransportKey = nameof(Transport);
        const Transport TransportDefault = Transport.RabbitMQ;
        public static Transport Transport => AppSettingsHelper.GetConfigEnum(TransportKey, TransportDefault);

        const string ActionTypesKey = "ActionTypesCsv";
        const MessageActionType ActionTypesDefault = MessageActionType.All;
        public static MessageActionType AllowedActionTypes => AppSettingsHelper.GetConfigEnum(ActionTypesKey, ActionTypesDefault);

        const string HandlerKey = "Handler";
        const string HandlerKeyDefault = "Cloud.MessageServer";
        public static string Handler => AppSettingsHelper.GetConfigStringValue(HandlerKey, HandlerKeyDefault);
    }
}
