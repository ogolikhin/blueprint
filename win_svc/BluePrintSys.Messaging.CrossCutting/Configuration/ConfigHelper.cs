﻿using BluePrintSys.Messaging.CrossCutting.Models.Enums;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.CrossCutting.Configuration
{
    public class ConfigHelper : IConfigHelper
    {
        public const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        public const string NServiceBusConnectionStringDefault = "";
        public string NServiceBusConnectionString => AppSettingsHelper.GetConfigStringValue(NServiceBusConnectionStringKey, NServiceBusConnectionStringDefault);

        public MessageBroker GetMessageBroker()
        {
            //Message Broker is determined by parsing the Connection String
            var connectionString = NServiceBusConnectionString;
            var connectionStringLower = connectionString.Replace(" ", "").Replace("\t", "").ToUpperInvariant();
            const string host = "HOST=";
            if (connectionStringLower.Contains(host))
            {
                return MessageBroker.RabbitMQ;
            }
            const string datasource = "DATASOURCE=";
            if (connectionStringLower.Contains(datasource))
            {
                return MessageBroker.SQL;
            }
            throw new InvalidConnectionStringException(I18NHelper.FormatInvariant("Invalid Connection String: {0}. It must contain {1} or {2}", connectionString, host, datasource));
        }

        public const string MessageQueueKey = "MessageQueue";
        public const string MessageQueueDefault = "Cloud.MessageServer";
        public string MessageQueue => AppSettingsHelper.GetConfigStringValue(MessageQueueKey, MessageQueueDefault);

        public const string ErrorQueueKey = "ErrorQueue";
        public const string ErrorQueueDefault = "errors";
        public string ErrorQueue => AppSettingsHelper.GetConfigStringValue(ErrorQueueKey, ErrorQueueDefault);

        public const string MessageProcessingMaxConcurrencyKey = "MessageProcessing.MaxConcurrency";
        public const int MessageProcessingMaxConcurrencyDefault = 1;
        public int MessageProcessingMaxConcurrency => AppSettingsHelper.GetConfigIntValue(MessageProcessingMaxConcurrencyKey, MessageProcessingMaxConcurrencyDefault);


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

        public const string SupportedActionTypesKey = "SupportedActionTypes";
        public const MessageActionType SupportedActionTypesDefault = MessageActionType.All;
        public MessageActionType SupportedActionTypes => AppSettingsHelper.GetConfigEnum(SupportedActionTypesKey, SupportedActionTypesDefault);

    }
}