using CefSharp.OffScreen;

namespace ImageRenderService.Tests.ImageGen
{
    public class TestableChromiumWebBrowser : ChromiumWebBrowser
    {
        public TestableChromiumWebBrowser()
            :base("",null,null, true)
        {
        }
    }
}
