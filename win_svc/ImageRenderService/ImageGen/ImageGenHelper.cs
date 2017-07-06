using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BluePrintSys.Messaging.CrossCutting.Logging;
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
        private static readonly int RenderWaitSeconds = ServiceHelper.BrowserRenderWaitTimeSeconds;

        public ImageGenHelper(IBrowserPool browserPool)
        {
            _browserPool = browserPool;
        }

        public async Task<MemoryStream> GenerateImageAsync(string processJsonModel, int maxImageWidth, int maxImageHeight, ImageFormat format)
        {
            Log.Info($"Started the image generation: maxImageWidth={maxImageWidth}, maxImageHeight={maxImageHeight}, format={format}");
            var browser = await _browserPool.Rent();
            if (browser == null)
            {
                return null;
            }

            var imageStream = new MemoryStream();
            try
            {
                await LoadPageAsync(browser, processJsonModel, maxImageWidth, maxImageHeight);

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
                    finally
                    {
                        _browserPool.Return(browser);
                    }

                }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                Log.Error($"The image generation failed - {ex.Message}");
                _browserPool.Return(browser);
                throw;
            }

            Log.Info($"Finished the image generation: length={imageStream.Length} bytes.");
            return imageStream;
        }

        //This is needed from rendering transparent background as black
        private Bitmap DrawImageOnWhiteBackground(IScreenshot screenshot, int width, int height)
        {
            Bitmap blank = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(blank);
            g.Clear(Color.White);
            g.DrawImage(screenshot.Image, 0, 0, width, height);
            return new Bitmap(blank);
        }

        public virtual async Task<bool> LoadPageAsync(IVirtualBrowser browser, string processJsonModel, int maxImageWidth, int maxImageHeight)
        {
            try
            {
                var task = new TaskCompletionSource<bool>();
                if(!_tcs.TryAdd(browser, task))
                {
                    Debug.Assert(false, "Unexpected Error: The dictionary does not contain a key (browser) added previously.");
                }

                var htmlPath = Path.GetFullPath("ProcessHtml/index.html");

                browser.AsyncBoundObject.Reset(RenderWaitSeconds * 1000);

                browser.LoadingStateChanged += BrowserLoadingStateChanged;
                browser.Load(htmlPath);

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
            // Get Process model
            browser.ExecuteScriptAsync($"window.renderGraph('{HttpUtility.JavaScriptStringEncode(processJsonModel)}', {maxImageWidth}, {maxImageHeight});");

            bool renderResult;
            try
            {
                renderResult = await browser.AsyncBoundObject.RenderCompletionSource.Task;
            }
            catch (TaskCanceledException)
            {
                throw new ApplicationException(ServiceHelper.RenderTimeoutErrorMessage);
            }
            if (!renderResult)
            {
                throw new ApplicationException(browser.AsyncBoundObject.ErrorMessage);
            }

            Log.Info($"Cef Image: width = {browser.AsyncBoundObject.Width}, height = {browser.AsyncBoundObject.Height}, scale = {browser.AsyncBoundObject.Scale}");

            var w = browser.AsyncBoundObject.Width + 20;
            var h = browser.AsyncBoundObject.Height + 20;

            browser.Size = new Size(w, h);

            //-----------------------------------------------------------------------------------------
            // The way how to detect when the browser resizing is completed, with the timeout.
            for (var i = 0; i < MaxWaitTimeSeconds * 1000 / DelayIntervalMilliseconds; i++)
            {

                if (browser.Bitmap != null && browser.Bitmap.Width == w && browser.Bitmap.Height == h)
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
                    // Disabling the scroll bars on 'body' element does not work.
                    // The scroll bars should be disabled on the Process container.
                    // But this causes shifting task headers and labels.
                    //browser.ExecuteScriptAsync("document.body.style.overflow = 'hidden'");

                    //Give the browser a little time to render
                    Thread.Sleep(RenderDelayMilliseconds);
                });

                _tcs[browser].SetResult(true);
            }
        }
    }
}
