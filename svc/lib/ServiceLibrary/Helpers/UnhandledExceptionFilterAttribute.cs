using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace ServiceLibrary.Helpers
{
    public class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpStatusCode? statusCode = null;

            if (context.Exception is NotImplementedException)
            {
                statusCode = HttpStatusCode.NotImplemented;
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
            }

            context.Response = context.Request.CreateErrorResponse(statusCode.Value, context.Exception.Message);
        }
    }
}