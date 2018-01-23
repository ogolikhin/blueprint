using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{
    public abstract class BaseCacheAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext.Response != null)
            {
                actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue();
                CustomizeHttpResponseHeaders(actionExecutedContext.Response.Headers);
            }
        }

        protected abstract void CustomizeHttpResponseHeaders(HttpResponseHeaders responseHeaders);
    }
}
