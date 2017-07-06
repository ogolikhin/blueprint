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
        public const string ServiceNameKey = "Service.Name";
        public const string ServiceNameDefault = "BlueprintActionHandler";
        public static string ServiceName => AppSettingsHelper.GetConfigStringValue(ServiceNameKey, ServiceNameDefault);

        public const string MessageProcessingMaxConcurrencyKey = "MessageProcessing.MaxConcurrency";
        public const int MessageProcessingMaxConcurrencyDefault = 1;
        public static int MessageProcessingMaxConcurrency => AppSettingsHelper.GetConfigIntValue(MessageProcessingMaxConcurrencyKey, MessageProcessingMaxConcurrencyDefault);

        public const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        public const string NServiceBusConnectionStringDefault = "host=titan.blueprintsys.net;username=admin;password=$admin2011";
        public static string NServiceBusConnectionString => AppSettingsHelper.GetConfigStringValue(NServiceBusConnectionStringKey, NServiceBusConnectionStringDefault);

        public const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        public const string NServiceBusInstanceIdDefault = "";
        public static string NServiceBusInstanceId => AppSettingsHelper.GetConfigStringValue(NServiceBusInstanceIdKey, NServiceBusInstanceIdDefault);

        public const string SingleTenancyConnectionStringKey = "SingleTenancyConnectionString";
        public const string SingleTenancyConnectionStringDefault = "";
        public static string SingleTenancyConnectionString => AppSettingsHelper.GetConfigStringValue(SingleTenancyConnectionStringKey, SingleTenancyConnectionStringDefault);

        public const string CacheExpirationMinutesKey = "CacheExpirationMinutes";
        public const int CacheExpirationMinutesDefault = 1440;
        public static int CacheExpirationMinutes => AppSettingsHelper.GetConfigIntValue(CacheExpirationMinutesKey, CacheExpirationMinutesDefault);

        public const string TenancyKey = nameof(Tenancy);
        public const Tenancy TenancyDefault = Tenancy.Single;
        public static Tenancy Tenancy => AppSettingsHelper.GetConfigEnum(TenancyKey, TenancyDefault);

        public const string TransportKey = nameof(Transport);
        public const Transport TransportDefault = Transport.RabbitMQ;
        public static Transport Transport => AppSettingsHelper.GetConfigEnum(TransportKey, TransportDefault);

        public const string ActionTypesKey = "ActionTypesCsv";
        public static readonly MessageActionType ActionTypesDefault = MessageActionType.All;
        public static MessageActionType AllowedActionTypes => AppSettingsHelper.GetConfigEnum(ActionTypesKey, ActionTypesDefault);
    }
}
