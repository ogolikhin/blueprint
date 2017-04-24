using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.OffScreen;

namespace ImageRenderService
{
    interface IBrowserPool
    {
        Task<ChromiumWebBrowser> Rent();
        void Return(ChromiumWebBrowser browser);
    }
}
