﻿namespace BlueprintSys.RC.ImageService.Helpers
{
    public static class ServiceConfiguration
    {
        public const string ServiceNameKey = "Service.Name";
        public const string DefaultServiceName = "BlueprintImageGen";

        public const string BrowserPoolEnabledKey = "BrowserPool.Enabled";
        public const bool DefaultBrowserPoolEnabled = true;

        public const string BrowserPoolMaxSizeKey = "BrowserPool.MaxSize";
        public const int DefaultBrowserPoolMaxSize = 10;

        public const string BrowserPoolWaitTimeSecondsKey = "BrowserPool.WaitTimeSeconds";
        public const int DefaultBrowserPoolWaitTimeSeconds = 10;

        public const string BrowserResizeEventMaxWaitTimeSecondsKey = "BrowserResizeEvent.MaxWaitTimeSeconds";
        public const int DefaultBrowserResizeEventMaxWaitTimeSeconds = 10;

        public const string BrowserResizeEventDelayIntervalMillisecondsKey = "BrowserResizeEvent.DelayIntervalMilliseconds";
        public const int DefaultBrowserResizeEventDelayIntervalMilliseconds = 10;

        public const string BrowserRenderDelayMillisecondsKey = "BrowserRender.DelayMilliseconds";
        public const int DefaultBrowserRenderDelayMilliseconds = 500;

        public const string NServiceBusConnectionStringKey = "NServiceBus.ConnectionString";
        public const string DefaultNServiceBusConnectionString = "Data Source=BlueprintDevDB;Initial Catalog=Raptor;Integrated Security=True;Max Pool Size=80";

        public const string NServiceBusCriticalErrorRetryDelayKey = "NServiceBus.CriticalErrorRetryDelay";
        public const int DefaultNServiceBusCriticalErrorRetryDelay = 2;

        public const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        public const string DefaultNServiceBusInstanceId = "";

        public const string BrowserRenderWaitTimeSecondsKey = "BrowserRender.WaitTimeSeconds";
        public const int DefaultBrowserRenderWaitTimeSeconds = 10;

        public const string NServiceBusCriticalErrorRetryCountKey = "NServiceBus.CriticalErrorRetryCount";
        public const int DefaultNServiceBusCriticalErrorRetryCount = 3;

        public const string NServiceBusIgnoreCriticalErrors = "NServiceBus.IgnoreCriticalErrors";
        public const bool DefaultNServiceBusIgnoreCriticalErrors = false;
    }
}
