using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace ImageRenderService.ImageGen
{
    public class VirtualBrowser : IVirtualBrowser
    {
        private ChromiumWebBrowser _browser;

        public VirtualBrowser()
        {
            _browser = new ChromiumWebBrowser();
        }

        public bool IsBrowserInitialized => _browser.IsBrowserInitialized;

        public Size Size
        {
            get { return _browser.Size; }
            set { _browser.Size = value; }
        }

        public IScreenshot Bitmap => new Screenshot(_browser.Bitmap);

        public event EventHandler BrowserInitialized
        {
            add { _browser.BrowserInitialized += value; }
            remove { _browser.BrowserInitialized -= value; }
        }


        private VirtualBrowserLoadingStateChangedEventArgs ConvertEventArgs(LoadingStateChangedEventArgs args)
        {

            return new VirtualBrowserLoadingStateChangedEventArgs
            {
                IsLoading = args.IsLoading,
                Browser = this,
                CanGoBack = args.CanGoBack,
                CanGoForward = args.CanGoForward,
                CanReload = args.CanReload
            };
        }

        private readonly Dictionary<EventHandler<VirtualBrowserLoadingStateChangedEventArgs>, 
            EventHandler<LoadingStateChangedEventArgs>> _loadingStateChangedDelegates = 
            new Dictionary<EventHandler<VirtualBrowserLoadingStateChangedEventArgs>, EventHandler<LoadingStateChangedEventArgs>>();
        public event EventHandler<VirtualBrowserLoadingStateChangedEventArgs> LoadingStateChanged
        {
            add {
                _loadingStateChangedDelegates[value] = (s, e) =>
                {
                    value(this, ConvertEventArgs(e));
                };
                _browser.LoadingStateChanged += _loadingStateChangedDelegates[value];
            }
            remove
            {
                _browser.LoadingStateChanged -= _loadingStateChangedDelegates[value];
                _loadingStateChangedDelegates.Remove(value);
            }
        }

        public void Load(string url)
        {
            _browser.Load(url);
        }

        public Task<IScreenshot> ScreenshotAsync(bool ignoreExistingScreenshot = false)
        {
            var tcs = new TaskCompletionSource<IScreenshot>();
            _browser.ScreenshotAsync(ignoreExistingScreenshot).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(new Screenshot(t.Result));
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        public Task<VirtualBrowserJavascriptResponse> EvaluateScriptAsync(string script, TimeSpan? timeout = null)
        {
            var tcs = new TaskCompletionSource<VirtualBrowserJavascriptResponse>();
            _browser.EvaluateScriptAsync(script, timeout).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(new VirtualBrowserJavascriptResponse
                    {
                        Result = t.Result.Result
                    });
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        public void ExecuteScriptAsync(string script)
        {
            _browser.ExecuteScriptAsync(script);
        }


        public void Dispose()
        {
            _browser.Dispose();
        }
    }

}
