using System.Configuration;

namespace ImageRenderService.Helpers
{
    public static class ServiceHelper
    {
        public static string GetConfigValue(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }
        public static string ServiceName => GetConfigValue(ServiceConfiguration.NameKey, ServiceConfiguration.DefaultServiceName);
    }
}
