using System;
using System.IO;
using System.Web.Http;
using System.Web.Http.SelfHost;
using CefSharp;
using ImageRenderService.Helpers;
using ImageRenderService.Transport;
using ImageRenderService.Logging;
using Topshelf;

namespace ImageRenderService.ImageGen
{
    public class ImageGenService : ServiceControl
    {
        private HttpSelfHostServer _server;
        private readonly HttpSelfHostConfiguration _config;
        public readonly Uri ServiceAddress = new Uri(@"http://localhost:5557");

        private static readonly string NServiceBusConnectionString = ServiceHelper.NServiceBusConnectionString;

        private static readonly BrowserPool BrowserPool = BrowserPool.Create();

        public IImageGenHelper ImageGenerator = new ImageGenHelper(BrowserPool);

        private readonly NServiceBusServer _nServiceBusServer = new NServiceBusServer();

        private ImageGenService()
        {
            _config = new HttpSelfHostConfiguration(ServiceAddress);
            _config.Routes.MapHttpRoute("Api",
                "api/{controller}",
                new { url = RouteParameter.Optional });
        }

        private static ImageGenService _instance;

        public static ImageGenService Instance => _instance ?? (_instance = new ImageGenService());

        public bool Start(HostControl hostControl)
        {
            // Add a Standard Log Listener to the Logger
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            Log.Info("ImageGen Service is starting...");

            var settings = new CefSettings
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "CefSharp\\Cache")
            };

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            _server = new HttpSelfHostServer(_config);
            _server.OpenAsync().Wait();

            _nServiceBusServer.Start(NServiceBusConnectionString).Wait();

            Log.Info("ImageGen Service is started.");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
                Log.Info("ImageGen Service is stopping...");

                _nServiceBusServer.Stop().Wait();

                _server.CloseAsync().Wait();
                _server.Dispose();
                BrowserPool.Dispose();

                Log.Info("ImageGen Service is stopped.");

                // Remove Log Listener
                Log4NetStandardLogListener.Clear();
                LogManager.Manager.ClearListeners();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return true;
        }
       
    }
}
