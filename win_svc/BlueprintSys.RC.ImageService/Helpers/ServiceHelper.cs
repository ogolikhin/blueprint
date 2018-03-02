using BluePrintSys.Messaging.CrossCutting;
using System;

namespace BlueprintSys.RC.ImageService.Helpers
{
    public static class ServiceHelper
    {
        public static string ServiceName
            => AppSettingsHelper.GetConfigStringValue(ServiceConfiguration.ServiceNameKey, ServiceConfiguration.DefaultServiceName);

        public static bool BrowserPoolEnabled => AppSettingsHelper.GetConfigBoolValue(ServiceConfiguration.BrowserPoolEnabledKey,
            ServiceConfiguration.DefaultBrowserPoolEnabled);

        public static int BrowserPoolMaxSize => AppSettingsHelper.GetConfigIntValue(ServiceConfiguration.BrowserPoolMaxSizeKey,
            ServiceConfiguration.DefaultBrowserPoolMaxSize);

        public static int BrowserPoolWaitTimeSeconds
            => AppSettingsHelper.GetConfigIntValue(ServiceConfiguration.BrowserPoolWaitTimeSecondsKey,
                ServiceConfiguration.DefaultBrowserPoolWaitTimeSeconds);

        public static int BrowserResizeEventMaxWaitTimeSeconds
            => AppSettingsHelper.GetConfigIntValue(ServiceConfiguration.BrowserResizeEventMaxWaitTimeSecondsKey,
                ServiceConfiguration.DefaultBrowserResizeEventMaxWaitTimeSeconds);

        public static int BrowserResizeEventDelayIntervalMilliseconds
            => AppSettingsHelper.GetConfigIntValue(ServiceConfiguration.BrowserResizeEventDelayIntervalMillisecondsKey,
                ServiceConfiguration.DefaultBrowserResizeEventDelayIntervalMilliseconds);

        public static int BrowserRenderDelayMilliseconds
            => AppSettingsHelper.GetConfigIntValue(ServiceConfiguration.BrowserRenderDelayMillisecondsKey,
                ServiceConfiguration.DefaultBrowserRenderDelayMilliseconds);

        public static string NServiceBusConnectionString
            => AppSettingsHelper.GetConfigStringValue(ServiceConfiguration.NServiceBusConnectionStringKey,
                ServiceConfiguration.DefaultNServiceBusConnectionString);

        public static TimeSpan NServiceBusCriticalErrorRetryDelay
            => TimeSpan.FromMinutes(
                AppSettingsHelper.GetConfigIntValue(
                    ServiceConfiguration.NServiceBusCriticalErrorRetryDelayKey,
                    ServiceConfiguration.DefaultNServiceBusCriticalErrorRetryDelay));

        public static string NServiceBusInstanceId
            => AppSettingsHelper.GetConfigStringValue(ServiceConfiguration.NServiceBusInstanceIdKey,
                ServiceConfiguration.DefaultNServiceBusInstanceId);

        public static int BrowserRenderWaitTimeSeconds
            => AppSettingsHelper.GetConfigIntValue(ServiceConfiguration.BrowserRenderWaitTimeSecondsKey,
                ServiceConfiguration.DefaultBrowserRenderWaitTimeSeconds);

        public static string RenderTimeoutErrorMessage => "The Process rendering did not complete withing the timeout period.";

        public static int NServiceBusCriticalErrorRetryCount
            => AppSettingsHelper.GetConfigIntValue(
                    ServiceConfiguration.NServiceBusCriticalErrorRetryCountKey,
                    ServiceConfiguration.DefaultNServiceBusCriticalErrorRetryCount);

        // Should not be used (default is false) - adding it just in case if recovery policy is not working
        public static bool NServiceBusIgnoreCriticalErrors
            => AppSettingsHelper.GetConfigBoolValue(
                ServiceConfiguration.NServiceBusIgnoreCriticalErrors,
                ServiceConfiguration.DefaultNServiceBusIgnoreCriticalErrors);
    }
}
