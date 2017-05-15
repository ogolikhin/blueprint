﻿using System;
using System.IO;
using System.Web.Http;
using System.Web.Http.SelfHost;
using CefSharp;
using Topshelf;

namespace ImageRenderService.ImageGen
{
    class ImageGenService : ServiceControl
    {
        private HttpSelfHostServer _server;
        private readonly HttpSelfHostConfiguration _config;
        public readonly Uri ServiceAddress = new Uri(@"http://localhost:5557");

        private static readonly BrowserPool BrowserPool = BrowserPool.Create();

        public readonly ImageGenHelper ImageGenerator = new ImageGenHelper(BrowserPool);

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

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
                _server.CloseAsync().Wait();
                _server.Dispose();
                BrowserPool.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return true;
        }
       
    }
}