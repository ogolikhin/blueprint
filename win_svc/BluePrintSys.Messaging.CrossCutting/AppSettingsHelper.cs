using System;
using System.Configuration;

namespace BluePrintSys.Messaging.CrossCutting
{
    public static class AppSettingsHelper
    {
        public static T GetConfigEnum<T>(string key, T defaultValue, bool ignoreCase = true) where T : struct
        {
            T enumValue;
            return Enum.TryParse(ConfigurationManager.AppSettings[key], ignoreCase, out enumValue) ? enumValue : defaultValue;
        }

        public static string GetConfigStringValue(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        public static int GetConfigIntValue(string key, int defaultValue)
        {
            var strValue = ConfigurationManager.AppSettings[key];
            int intValue;
            if (string.IsNullOrWhiteSpace(strValue) || !int.TryParse(strValue, out intValue))
            {
                intValue = defaultValue;
            }
            return intValue;
        }

        public static bool GetConfigBoolValue(string key, bool defaultValue)
        {
            var strValue = ConfigurationManager.AppSettings[key];
            bool boolValue;
            if (string.IsNullOrWhiteSpace(strValue) || !bool.TryParse(strValue, out boolValue))
            {
                boolValue = defaultValue;
            }
            return boolValue;
        }

        public static string GetConnectionStringValue(string key)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[key];
            return connectionString?.ConnectionString;
        }
    }
}
