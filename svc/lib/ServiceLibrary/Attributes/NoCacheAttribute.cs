using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{
    public class NoCacheAttribute : BaseCacheAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            CustomizeHttpResponseHeaders(actionExecutedContext.Response.Headers);
        }

        protected override void CustomizeHttpResponseHeaders(HttpResponseHeaders responseHeaders)
        {
            // HTTP 1.1.
            responseHeaders.CacheControl.NoCache = true;
            responseHeaders.CacheControl.NoStore = true;
            responseHeaders.CacheControl.MustRevalidate = true;
            // HTTP 1.0.
            responseHeaders.Add("Pragma", "no-cache"); // HTTP 1.0.
        }
    }
}
