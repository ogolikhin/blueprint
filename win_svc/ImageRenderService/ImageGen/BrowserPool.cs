using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    public class BrowserPool : IBrowserPool, IDisposable
    {
        private static BrowserPool _instance;
        private const int MaxWaitTimeSeconds = 10;

        private BrowserPool()
        {
            
        }

        public static BrowserPool Create()
        {
            _instance = new BrowserPool
            {
                _freeBrowsers = new ConcurrentBag<IVirtualBrowser>()
            };
            _instance._browserPool = new Semaphore(_instance.MAXSIZE, _instance.MAXSIZE);
            return _instance;
        }

        private ConcurrentBag<IVirtualBrowser> _freeBrowsers;
        private readonly int MAXSIZE = 3;
        private Semaphore _browserPool;

        public async Task<IVirtualBrowser> Rent()
        {
            if (!_browserPool.WaitOne(MaxWaitTimeSeconds*1000))
            {
                return null;
            }

            IVirtualBrowser browser;
            //if there is a free browser - use it
            if (_freeBrowsers.TryTake(out browser))
            {
                return browser;
            }

            //create a new browser
            browser = new VirtualBrowser();

            //initialize it
            if (!browser.IsBrowserInitialized)
            {
                var tcs = new TaskCompletionSource<bool>();
                browser.BrowserInitialized += delegate {
                    tcs.SetResult(true);
                };
                await tcs.Task;
            }
            browser.Size = new Size(1920, 1080);
            //and use it
            return browser;
        }

        public void Return(IVirtualBrowser browser)
        {
            _freeBrowsers.Add(browser);
            _browserPool.Release(1);
        }

        public void Dispose()
        {
            foreach (var chromiumWebBrowser in _freeBrowsers)
            {
                chromiumWebBrowser.Dispose();
            }
        }
    }
}
