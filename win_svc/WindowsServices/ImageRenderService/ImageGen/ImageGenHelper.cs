using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace ImageRenderService.ImageGen
{
    public class ImageGenHelper : IImageGenHelper
    {
        private readonly IBrowserPool _browserPool;
        private readonly Dictionary<ChromiumWebBrowser, TaskCompletionSource<bool>> 
            _tcs = new Dictionary<ChromiumWebBrowser, TaskCompletionSource<bool>>();

        public ImageGenHelper(IBrowserPool browserPool)
        {
            _browserPool = browserPool;
        }

        public async Task<MemoryStream> GenerateImageAsync(string url, ImageFormat format)
        {
            ChromiumWebBrowser browser = await _browserPool.Rent();

            await LoadPageAsync(browser, url);

            // Wait for the screen shot to be taken.
            var task = browser.ScreenshotAsync();
            MemoryStream imageStream = new MemoryStream();
            await task.ContinueWith(x =>
            {
                if (format.Equals(ImageFormat.Jpeg))
                {
                    Bitmap tempImage = DrawImageOnWhiteBackground(task.Result, 1920, 1080);
                    tempImage.Save(imageStream, ImageFormat.Jpeg);
                    tempImage.Dispose();
                }
                else
                {
                    task.Result.Save(imageStream, ImageFormat.Png);
                }
                //We no longer need the Bitmap.
                // Dispose it to avoid keeping the memory alive.  Especially important in 32 - bit applications.
                task.Result.Dispose();

                _browserPool.Return(browser);
                
            }, TaskScheduler.Default);
            return imageStream;
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
