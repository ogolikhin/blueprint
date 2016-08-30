using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace ServiceLibrary.Attributes
{
    public class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is NotImplementedException)
            {
                var statusCode = HttpStatusCode.NotImplemented;
                context.Response = context.Request.CreateErrorResponse(statusCode, context.Exception.Message);
            }
            else
            {
                base.OnException(context);
            }
        }
    }
}
