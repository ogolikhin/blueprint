using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    public class VirtualBrowserLoadingStateChangedEventArgs
    {
        public bool IsLoading { get; set; }
    }

    public class VirtualBrowserJavascriptResponse
    {
        public object Result { get; set; }
    }

    public interface IVirtualBrowser : IDisposable
    {
        bool IsBrowserInitialized { get; }
        Size Size { get; set; }
        Bitmap Bitmap { get; }

        event EventHandler BrowserInitialized;
        event EventHandler<VirtualBrowserLoadingStateChangedEventArgs> LoadingStateChanged;

        void Load(string url);
        Task<Bitmap> ScreenshotAsync(bool ignoreExistingScreenshot = false);
        Task<VirtualBrowserJavascriptResponse> EvaluateScriptAsync(string script, TimeSpan? timeout = default(TimeSpan?));
        void ExecuteScriptAsync(string script);
    }
}
