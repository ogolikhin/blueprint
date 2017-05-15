namespace ImageRenderService.Helpers
{
    public static class ServiceConfiguration
    {
        public const string ServiceNameKey = "Service.Name";
        public const string DefaultServiceName = "BlueprintImageGen";

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
        public const string DefaultNServiceBusConnectionString = "host=titan.blueprintsys.net;username=admin;password=$admin2011";

        public const string NServiceBusInstanceIdKey = "NServiceBus.InstanceId";
        public const string DefaultNServiceBusInstanceId = "1";
    }
}
