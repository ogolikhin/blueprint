using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using CefSharp;
using CefSharp.OffScreen;
using Topshelf;


namespace ImageRenderService
{
    class ImageGenService : ServiceControl
    {
        private HttpSelfHostServer _server;
        private readonly HttpSelfHostConfiguration _config;
        public Uri ServiceAddress = new Uri(@"http://localhost:5555");

        private IBrowserPool _browserPool = BrowserPool.Create();

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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return true;
        }

        //--------------------------------------------------------

        private readonly Dictionary<ChromiumWebBrowser, TaskCompletionSource<bool>> _tcs = new Dictionary<ChromiumWebBrowser, TaskCompletionSource<bool>>();

        public async Task<byte[]> GenerateImageAsync(string url, ImageFormat format)
        {
            byte[] image = null;
            ChromiumWebBrowser browser = await _browserPool.Rent();

            await LoadPageAsync(browser, url);

            // Wait for the screen shot to be taken.
            var task = browser.ScreenshotAsync();
            await task.ContinueWith(x =>
            {
                using (var ms = new MemoryStream())
                {
                    if (format.Equals(ImageFormat.Jpeg))
                    {
                        Bitmap tempImage = DrawImageOnWhiteBackground(task.Result, 1920, 1080);
                        tempImage.Save(ms, ImageFormat.Jpeg);
                        tempImage.Dispose();
                    }
                    else
                    {
                        task.Result.Save(ms, ImageFormat.Png);    
                    }
                    image = ms.ToArray();
                }

                //We no longer need the Bitmap.
                // Dispose it to avoid keeping the memory alive.  Especially important in 32 - bit applications.
                task.Result.Dispose();
            }, TaskScheduler.Default);

            //return browser to the pool
            _browserPool.Return(browser);

            return image;
        }

        //This is needed from rendering transparent background as black
        private Bitmap DrawImageOnWhiteBackground(Bitmap image, int width, int height)
        {
            Bitmap blank = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(blank);
            g.Clear(Color.White);
            g.DrawImage(image, 0, 0, width, height);
            return new Bitmap(blank);
        }

        private async Task<bool> LoadPageAsync(ChromiumWebBrowser browser, string address)
        {
            
            var task = new TaskCompletionSource<bool>();
            _tcs.Add(browser, task);

            var isProcessFile = string.IsNullOrWhiteSpace(address);

            var effectiveAddress = isProcessFile ? Path.GetFullPath("process.html") : address;

            browser.Load(effectiveAddress);
            browser.LoadingStateChanged += BrowserLoadingStateChanged;
            await task.Task;
            _tcs.Remove(browser);

            //-------------------------------------------
            // Set the browser size.
            string width;
            string height;
            if (isProcessFile)
            {
                width = "document.body.firstElementChild.firstElementChild.clientWidth";
                height = "document.body.firstElementChild.firstElementChild.clientHeight";
            }
            else
            {
                width = "document.body.clientWidth";
                height = "document.body.clientHeight";
            }

            var w = await browser.EvaluateScriptAsync(width);
            var h = await browser.EvaluateScriptAsync(height);

            var eW = ((int)w.Result) + 10;
            var eH = (int)h.Result;


            if (w.Result != null && h.Result != null)
            {
                browser.Size = new Size(eW, eH);
            }

            //-----------------------------------------------------------------------------------------
            // The way how to detect when the browser resizing is completed, with the timeout ~ 10 sec.
            var i = 10;
            while (true)
            {
                if ((browser.Bitmap.Width == eW && browser.Bitmap.Height == eH)
                    || i > 1000)
                {
                    break;
                }
                Thread.Sleep(10);
                i++;
            }
            //-----------------------------------------------------------------------------------------

            return true;
        }

        private async void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            var browser = sender as ChromiumWebBrowser;
            // Check to see if loading is complete - this event is called twice, one when loading starts, second time when it's finished.
            if (browser != null && !e.IsLoading)
            {
                // Remove the load event handler, because we only want one snapshot of the initial page.
                browser.LoadingStateChanged -= BrowserLoadingStateChanged;

                var scriptTask =
                    browser.EvaluateScriptAsync("document.getElementById('lst-ib').value = 'CefSharp Was Here!'");

                await scriptTask.ContinueWith(t =>
                {
                    browser.ExecuteScriptAsync("document.body.style.overflow = 'hidden'");

                    //Give the browser a little time to render
                    Thread.Sleep(500);
                });

                _tcs[browser].SetResult(true);
            }
        }
    }
}
