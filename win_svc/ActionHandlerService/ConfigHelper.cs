using System;
using System.Collections.Generic;
using System.Configuration;
using BluePrintSys.ActionMessaging.Models;

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
        public static string ServiceName => GetConfigStringValue(ServiceNameKey, ServiceNameDefault);

        public const string MessageProcessingMaxConcurrencyKey = "MessageProcessing.MaxConcurrency";
        public const int MessageProcessingMaxConcurrencyDefault = 1;
        public static int MessageProcessingMaxConcurrency => GetConfigIntValue(MessageProcessingMaxConcurrencyKey, MessageProcessingMaxConcurrencyDefault);

        public const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        public const string NServiceBusConnectionStringDefault = "host=titan.blueprintsys.net;username=admin;password=$admin2011";
        public static string NServiceBusConnectionString => GetConfigStringValue(NServiceBusConnectionStringKey, NServiceBusConnectionStringDefault);

        public const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        public const string NServiceBusInstanceIdDefault = "";
        public static string NServiceBusInstanceId => GetConfigStringValue(NServiceBusInstanceIdKey, NServiceBusInstanceIdDefault);

        public const string SingleTenancyConnectionStringKey = "SingleTenancyConnectionString";
        public const string SingleTenancyConnectionStringDefault = "";
        public static string SingleTenancyConnectionString => GetConfigStringValue(SingleTenancyConnectionStringKey, SingleTenancyConnectionStringDefault);

        public const string CacheExpirationMinutesKey = "CacheExpirationMinutes";
        public const int CacheExpirationMinutesDefault = 1440;
        public static int CacheExpirationMinutes => GetConfigIntValue(CacheExpirationMinutesKey, CacheExpirationMinutesDefault);

        public const string TenancyKey = nameof(Tenancy);
        public const Tenancy TenancyDefault = Tenancy.Single;
        public static Tenancy Tenancy => GetConfigEnum(TenancyKey, TenancyDefault);

        public const string TransportKey = nameof(Transport);
        public const Transport TransportDefault = Transport.RabbitMQ;
        public static Transport Transport => GetConfigEnum(TransportKey, TransportDefault);

        public const string ActionTypesKey = "ActionTypes";
        public static readonly List<MessageActionType> AllAllowedActionTypes = new List<MessageActionType>
        {
            MessageActionType.Notification,
            MessageActionType.GenerateDescendants,
            MessageActionType.GenerateTests,
            MessageActionType.GenerateUserStories
        };
        public static List<MessageActionType> AllowedActionTypes => GetConfigEnumList(ActionTypesKey, AllAllowedActionTypes);

        private static T GetConfigEnum<T>(string key, T defaultValue) where T : struct
        {
            T enumValue;
            return Enum.TryParse(ConfigurationManager.AppSettings[key], true, out enumValue) ? enumValue : defaultValue;
        }

        private static List<T> GetConfigEnumList<T>(string key, List<T> allAllowedValues) where T : struct
        {
            var configValue = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(configValue))
            {
                return allAllowedValues;
            }
            var enumValues = new List<T>();
            foreach (var enumString in configValue.Split(','))
            {
                T enumValue;
                if (Enum.TryParse(enumString, true, out enumValue) && allAllowedValues.Contains(enumValue))
                {
                    enumValues.Add(enumValue);
                }
                else
                {
                    return allAllowedValues;
                }
            }
            return enumValues;
        }

        private static string GetConfigStringValue(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        private static int GetConfigIntValue(string key, int defaultValue)
        {
            var strValue = ConfigurationManager.AppSettings[key];
            int intValue;
            if (string.IsNullOrWhiteSpace(strValue) || !int.TryParse(strValue, out intValue))
            {
                intValue = defaultValue;
            }
            return intValue;
        }
    }
}
