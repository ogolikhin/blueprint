﻿using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using BlueprintSys.RC.ImageService.Helpers;

namespace BlueprintSys.RC.ImageService.ImageGen
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public class BrowserPool : IBrowserPool, IDisposable
    {
        private static BrowserPool _instance;
        private readonly int _maxWaitTimeSeconds = ServiceHelper.BrowserPoolWaitTimeSeconds;

        private ConcurrentBag<IVirtualBrowser> _freeBrowsers;
        private static readonly int MaxSize = ServiceHelper.BrowserPoolMaxSize;
        // If the pool is not enabled, the pool acts just as a factory.
        private static readonly bool IsPoolEnabled = ServiceHelper.BrowserPoolEnabled;
        private SemaphoreSlim _browserPool;

        private BrowserPool()
        {
        }

        public static BrowserPool Create()
        {
            _instance = new BrowserPool
            {
                _freeBrowsers = IsPoolEnabled ? new ConcurrentBag<IVirtualBrowser>() : null,
                _browserPool = new SemaphoreSlim(MaxSize, MaxSize)
            };
            return _instance;
        }

        public async Task<IVirtualBrowser> Rent()
        {
            if (!_browserPool.Wait(_maxWaitTimeSeconds * 1000))
            {
                return null;
            }

            IVirtualBrowser browser;
            // If there is a free browser - use it
            if (IsPoolEnabled && _freeBrowsers.TryTake(out browser))
            {
                return browser;
            }

            // Create a new browser
            browser = new VirtualBrowser();

            // Initialize it
            if (!browser.IsBrowserInitialized)
            {
                var tcs = new TaskCompletionSource<bool>();
                browser.BrowserInitialized += delegate {
                    tcs.SetResult(true);
                };
                await tcs.Task;
            }
            browser.Size = new Size(1920, 1080);
            // and use it
            return browser;
        }

        public void Return(IVirtualBrowser browser)
        {
            if (IsPoolEnabled)
            {
                browser.Size = new Size(1920, 1080);
                _freeBrowsers.Add(browser);
            }
            else
            {
                browser.Dispose();
            }

            _browserPool.Release(1);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_browserPool")]
        public void Dispose()
        {
            if (!IsPoolEnabled)
            {
                return;
            }

            foreach (var chromiumWebBrowser in _freeBrowsers)
            {
                chromiumWebBrowser?.Dispose();
            }
        }
    }
}
