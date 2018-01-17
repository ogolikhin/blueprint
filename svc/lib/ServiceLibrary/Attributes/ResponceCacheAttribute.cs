using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{

    public class ResponceCacheAttribute : ActionFilterAttribute
    {
        // <summary>
        // Specifies the maximum amount of time in seconds a resource will be considered fresh.
        // </summary>
        private readonly int _duration;

        public ResponceCacheAttribute(int duration)
        {
            _duration = duration;
        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(_duration)
            };
        }
    }
}