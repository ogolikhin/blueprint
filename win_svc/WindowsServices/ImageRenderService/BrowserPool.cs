using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using CefSharp.OffScreen;

namespace ImageRenderService
{
    class BrowserPool : IBrowserPool
    {
        private static BrowserPool _instance;

        private BrowserPool()
        {
            
        }

        public static BrowserPool Create()
        {
            _instance = new BrowserPool();
            _instance._freeBrowsers = new ConcurrentBag<ChromiumWebBrowser>();
            _instance._browserPool = new Semaphore(_instance.MAXSIZE, _instance.MAXSIZE);
            return _instance;
        }

        private ConcurrentBag<ChromiumWebBrowser> _freeBrowsers;
        private readonly int MAXSIZE = 3;
        private Semaphore _browserPool;

        private TaskCompletionSource<bool> _tcs;

        public async Task<ChromiumWebBrowser> Rent()
        {
            _browserPool.WaitOne();
            
            ChromiumWebBrowser browser;
            //if there is a free browser - use it
            if (_freeBrowsers.TryTake(out browser))
            {
                return browser;
            }

            //create a new browser
            browser = new ChromiumWebBrowser();

            //initialize it
            if (!browser.IsBrowserInitialized)
            {
                _tcs = new TaskCompletionSource<bool>();
                browser.BrowserInitialized += delegate {
                    _tcs.SetResult(true);
                };
                await _tcs.Task;
            }
            browser.Size = new Size(1920, 1080);
            //and use it
            return browser;
        }

        public void Return(ChromiumWebBrowser browser)
        {
            _freeBrowsers.Add(browser);
            _browserPool.Release(1);
        }
    }
}
