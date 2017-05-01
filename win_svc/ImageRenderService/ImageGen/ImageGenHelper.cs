using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageRenderService.Helpers;

namespace ImageRenderService.ImageGen
{
    public class ImageGenHelper : IImageGenHelper
    {
        private readonly IBrowserPool _browserPool;
        private readonly ConcurrentDictionary<IVirtualBrowser, TaskCompletionSource<bool>> 
            _tcs = new ConcurrentDictionary<IVirtualBrowser, TaskCompletionSource<bool>>();

        private static readonly int MaxWaitTimeSeconds = ServiceHelper.BrowserResizeEventMaxWaitTimeSeconds;
        private static readonly int DelayIntervalMilliseconds = ServiceHelper.BrowserResizeEventDelayIntervalMilliseconds;
        private static readonly int RenderDelayMilliseconds = ServiceHelper.BrowserRenderDelayMilliseconds;

        public ImageGenHelper(IBrowserPool browserPool)
        {
            _browserPool = browserPool;
        }

        public async Task<MemoryStream> GenerateImageAsync(string url, ImageFormat format)
        {
            var browser = await _browserPool.Rent();
            if (browser == null)
            {
                return null;
            }

            var imageStream = new MemoryStream();
            try
            {
                await LoadPageAsync(browser, url);

                // Wait for the screen shot to be taken.
                var task = browser.ScreenshotAsync();
                await task.ContinueWith(x =>
                {
                    try
                    {
                        if (format.Equals(ImageFormat.Jpeg))
                        {
                            var tempImage = DrawImageOnWhiteBackground(task.Result, 1920, 1080);
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
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    finally
                    {
                        _browserPool.Return(browser);
                    }

                }, TaskScheduler.Default);
            }
            catch (Exception e2)
            {
                Console.WriteLine(e2);
                _browserPool.Return(browser);
                throw;
            }
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

        private async Task<bool> LoadPageAsync(IVirtualBrowser browser, string address)
        {
            bool isProcessFile;
            try
            {
                var task = new TaskCompletionSource<bool>();
                if(!_tcs.TryAdd(browser, task))
                {
                    Debug.Assert(false, "Unexpected Error: The dictionary does not contain a key (browser) added previously.");
                }

                isProcessFile = string.IsNullOrWhiteSpace(address);

                var effectiveAddress = isProcessFile ? Path.GetFullPath("process.html") : address;

                browser.Load(effectiveAddress);
                browser.LoadingStateChanged += BrowserLoadingStateChanged;
                await task.Task;
            }
            finally
            {
                TaskCompletionSource<bool> removedTask;
                if (!_tcs.TryRemove(browser, out removedTask))
                {
                    Debug.Assert(false, "Unexpected Error: The dictionary already contains a key (browser).");
                }
            }

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
            for (var i = 0; i < MaxWaitTimeSeconds * 1000 / DelayIntervalMilliseconds; i++)
            {

                if (browser.Bitmap != null && browser.Bitmap.Width == eW && browser.Bitmap.Height == eH)
                {
                    break;
                }
                await Task.Delay(DelayIntervalMilliseconds);
            }
           
            //-----------------------------------------------------------------------------------------

            return true;
        }

        private async void BrowserLoadingStateChanged(object sender, VirtualBrowserLoadingStateChangedEventArgs e)
        {
            var browser = sender as IVirtualBrowser;
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
                    Thread.Sleep(RenderDelayMilliseconds);
                });

                _tcs[browser].SetResult(true);
            }
        }
    }
}
