﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using BlueprintSys.RC.ImageService.Helpers;
using BlueprintSys.RC.ImageService.Logging;
using BlueprintSys.RC.ImageService.Transport;
using BluePrintSys.Messaging.CrossCutting.Logging;
using CefSharp;
using Topshelf;

namespace BlueprintSys.RC.ImageService.ImageGen
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ImageGenService : ServiceControl
    {
        private HttpSelfHostServer _server;
        private readonly HttpSelfHostConfiguration _config;
        public readonly Uri ServiceAddress = new Uri(@"http://localhost:5557");

        private static readonly string NServiceBusConnectionString = ServiceHelper.NServiceBusConnectionString;

        private static readonly BrowserPool BrowserPool = BrowserPool.Create();

        public IImageGenHelper ImageGenerator = new ImageGenHelper(BrowserPool);

        private readonly NServiceBusServer _nServiceBusServer = new NServiceBusServer();

        private volatile bool _stoppingService;

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
                // By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "CefSharp\\Cache")
            };

            // Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            // Keep it for testing purposes but make it safe.
            try
            {
                _server = new HttpSelfHostServer(_config);
                _server.OpenAsync().Wait();
            }
            catch (Exception e)
            {
                // Debug in order that customers do not see it in the production
                Log.DebugFormat("Failed to start self host web server.", e);
            }

            Task.Run(() => _nServiceBusServer.Start(NServiceBusConnectionString, () => {
                Log.Error("ImageGen service - restarting after critical error");
                if (Environment.UserInteractive)
                {
                    hostControl.Stop();
                }
                else
                {
                    Stop(hostControl);
                    Environment.FailFast("ImageGen service - NSB critical error");
                }
            })).ContinueWith(startTask =>
                {
                    if (!string.IsNullOrEmpty(startTask.Result))
                    {
                        Log.Error(startTask.Result);
                        hostControl.Stop();
                    }
                });

            Log.Info("ImageGen Service is started.");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            lock (this)
            {
                if (_stoppingService)
                {
                    return true;
                }
                _stoppingService = true;
            }

            try
            {
                Log.Info("ImageGen Service is stopping...");

                _nServiceBusServer.Stop().Wait();

                // Keep it for testing purposes but make it safe.
                try
                {
                    _server.CloseAsync().Wait();
                    _server.Dispose();
                }
                catch (Exception e)
                {
                    // Debug in order that customers do not see it in the production
                    Log.DebugFormat("Failed to stop self host web server.", e);
                }

                BrowserPool.Dispose();

                Log.Info("ImageGen Service is stopped.");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                // Remove Log Listener
                Log4NetStandardLogListener.Clear();
                LogManager.Manager.ClearListeners();
            }

            return true;
        }
    }
}
