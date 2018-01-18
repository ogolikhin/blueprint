using System.Net.Http.Headers;

namespace ServiceLibrary.Attributes
{
    public class NoCacheAttribute : ResponseCacheAttribute
    {
        protected override void CustomizeHttpResponseHeaders(HttpResponseHeaders responseHeaders)
        {
            base.CustomizeHttpResponseHeaders(responseHeaders);
            // HTTP 1.1.
            responseHeaders.CacheControl.NoCache = true;
            responseHeaders.CacheControl.NoStore = true;
            responseHeaders.CacheControl.MustRevalidate = true;
            // HTTP 1.0.
            responseHeaders.Add("Pragma", "no-cache"); // HTTP 1.0.
        }
    }
}
