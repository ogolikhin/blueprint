using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{

    public class ResponseCacheAttribute : BaseCacheAttribute
    {
        private int? _duration;

        // <summary>
        // Specifies the maximum amount of time in seconds a resource will be considered fresh.
        // </summary>
        public int Duration
        {
            get { return _duration ?? 0; }
            set { _duration = value; }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext.Response != null)
            {
                CustomizeHttpResponseHeaders(actionExecutedContext.Response.Headers);
            }
        }

        protected override void CustomizeHttpResponseHeaders(HttpResponseHeaders responseHeaders)
        {
            responseHeaders.CacheControl = new CacheControlHeaderValue();
            if (_duration.HasValue)
            {
                responseHeaders.CacheControl.MaxAge = TimeSpan.FromSeconds(Duration);
            }
        }

    }
}