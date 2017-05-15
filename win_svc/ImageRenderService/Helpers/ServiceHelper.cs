using System.Configuration;

namespace ImageRenderService.Helpers
{
    public static class ServiceHelper
    {
        #region Public Properties

        public static string ServiceName
            => GetConfigStringValue(ServiceConfiguration.ServiceNameKey, ServiceConfiguration.DefaultServiceName);

        public static int BrowserPoolMaxSize => GetConfigIntValue(ServiceConfiguration.BrowserPoolMaxSizeKey,
            ServiceConfiguration.DefaultBrowserPoolMaxSize);

        public static int BrowserPoolWaitTimeSeconds
            => GetConfigIntValue(ServiceConfiguration.BrowserPoolWaitTimeSecondsKey,
                ServiceConfiguration.DefaultBrowserPoolWaitTimeSeconds);

        public static int BrowserResizeEventMaxWaitTimeSeconds
            => GetConfigIntValue(ServiceConfiguration.BrowserResizeEventMaxWaitTimeSecondsKey,
                ServiceConfiguration.DefaultBrowserResizeEventMaxWaitTimeSeconds);

        public static int BrowserResizeEventDelayIntervalMilliseconds
            => GetConfigIntValue(ServiceConfiguration.BrowserResizeEventDelayIntervalMillisecondsKey,
                ServiceConfiguration.DefaultBrowserResizeEventDelayIntervalMilliseconds);

        public static int BrowserRenderDelayMilliseconds
            => GetConfigIntValue(ServiceConfiguration.BrowserRenderDelayMillisecondsKey,
                ServiceConfiguration.DefaultBrowserRenderDelayMilliseconds);

        public static string NServiceBusConnectionString
            => GetConfigStringValue(ServiceConfiguration.NServiceBusConnectionStringKey,
                ServiceConfiguration.DefaultNServiceBusConnectionString);

        public static string NServiceBusInstanceId
            => GetConfigStringValue(ServiceConfiguration.NServiceBusInstanceIdKey,
                ServiceConfiguration.DefaultNServiceBusInstanceId);

        #endregion


        #region Private Methods

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

        #endregion

    }
}
