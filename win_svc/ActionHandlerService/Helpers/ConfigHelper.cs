﻿using ActionHandlerService.Models.Enums;
using BluePrintSys.Messaging.CrossCutting;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.Helpers
{
    public interface IConfigHelper
    {
        MessageActionType SupportedActionTypes { get; }
        string MessageQueue { get; }
        string ErrorQueue { get; }
        int MessageProcessingMaxConcurrency { get; }
        MessageBroker MessageBroker { get; }
        Tenancy Tenancy { get; }
        int CacheExpirationMinutes { get; }
        string SingleTenancyConnectionString { get; }
        string NServiceBusConnectionString { get; }
        string NServiceBusInstanceId { get; }
        string ServiceName { get; }
    }

    public class ConfigHelper : IConfigHelper
    {
        public const string ServiceNameKey = "Service.Name";
        public const string ServiceNameDefault = "BlueprintActionHandler";
        public string ServiceName => AppSettingsHelper.GetConfigStringValue(ServiceNameKey, ServiceNameDefault);

        public const string MessageProcessingMaxConcurrencyKey = "MessageProcessing.MaxConcurrency";
        public const int MessageProcessingMaxConcurrencyDefault = 1;
        public int MessageProcessingMaxConcurrency => AppSettingsHelper.GetConfigIntValue(MessageProcessingMaxConcurrencyKey, MessageProcessingMaxConcurrencyDefault);

        public const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        public const string NServiceBusConnectionStringDefault = "";
        public string NServiceBusConnectionString => AppSettingsHelper.GetConfigStringValue(NServiceBusConnectionStringKey, NServiceBusConnectionStringDefault);

        public const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        public const string NServiceBusInstanceIdDefault = "";
        public string NServiceBusInstanceId => AppSettingsHelper.GetConfigStringValue(NServiceBusInstanceIdKey, NServiceBusInstanceIdDefault);

        public const string SingleTenancyConnectionStringKey = "TenantsDatabase";
        public string SingleTenancyConnectionString => AppSettingsHelper.GetConnectionStringValue(SingleTenancyConnectionStringKey);

        public const string CacheExpirationMinutesKey = "CacheExpirationMinutes";
        public const int CacheExpirationMinutesDefault = 1440;
        public int CacheExpirationMinutes => AppSettingsHelper.GetConfigIntValue(CacheExpirationMinutesKey, CacheExpirationMinutesDefault);

        public const string TenancyKey = "Tenancy";
        public const Tenancy TenancyDefault = Tenancy.Single;
        public Tenancy Tenancy => AppSettingsHelper.GetConfigEnum(TenancyKey, TenancyDefault);

        public const string MessageBrokerKey = "MessageBroker";
        public const MessageBroker MessageBrokerDefault = MessageBroker.RabbitMQ;
        public MessageBroker MessageBroker => AppSettingsHelper.GetConfigEnum(MessageBrokerKey, MessageBrokerDefault);

        public const string SupportedActionTypesKey = "SupportedActionTypes";
        public const MessageActionType SupportedActionTypesDefault = MessageActionType.All;
        public MessageActionType SupportedActionTypes => AppSettingsHelper.GetConfigEnum(SupportedActionTypesKey, SupportedActionTypesDefault);

        public const string MessageQueueKey = "MessageQueue";
        public const string MessageQueueDefault = "Cloud.MessageServer";
        public string MessageQueue => AppSettingsHelper.GetConfigStringValue(MessageQueueKey, MessageQueueDefault);

        public const string ErrorQueueKey = "ErrorQueue";
        public const string ErrorQueueDefault = "errors";
        public string ErrorQueue => AppSettingsHelper.GetConfigStringValue(ErrorQueueKey, ErrorQueueDefault);
    }
}
