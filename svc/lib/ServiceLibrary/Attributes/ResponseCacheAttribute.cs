using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{

    public class ResponseCacheAttribute : BaseCacheAttribute
    {

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext.Response != null)
            {
                base.CustomizeHttpResponseHeaders(actionExecutedContext.Response.Headers);
            }
        }

    }
}