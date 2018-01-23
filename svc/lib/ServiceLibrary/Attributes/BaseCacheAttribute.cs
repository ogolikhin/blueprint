using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{
    public class BaseCacheAttribute : ActionFilterAttribute
    {

        protected virtual void CustomizeHttpResponseHeaders(HttpResponseHeaders responseHeaders)
        {
        }
    }
}
