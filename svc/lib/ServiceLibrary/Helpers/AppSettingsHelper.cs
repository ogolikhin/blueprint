using System;
using System.Configuration;
using System.Globalization;

namespace ServiceLibrary.Helpers
{
    public class AppSettingsHelper
    {
        public static T TryGetConfigurationValue<T>(string appSettingsKey, T defaultValue = default(T))
        {

            var appSettingValue = ConfigurationManager.AppSettings[appSettingsKey];

            if (appSettingValue == null)
            {
                return defaultValue;
            }

            var value = (T)Convert.ChangeType(appSettingValue, typeof(T), CultureInfo.InvariantCulture);

            if (value == null)
            {
                return defaultValue;
            }
            return value;
        }
    }
}
