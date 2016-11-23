using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.ConfigControl;

namespace ServiceLibrary.Attributes
{
    public class BaseExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private const string UnknownLogSource = "Unknown";

        public override async Task OnExceptionAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            var ex = context.Exception;
            HttpStatusCode statusCode;
            int? errorCode = null;

            if (ex is AuthenticationException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = ((AuthenticationException)ex).ErrorCode;
            }
            else if (ex is NotImplementedException)
            {
                statusCode = HttpStatusCode.NotImplemented;
            }
            else if (ex is BadRequestException)
            {
                statusCode = HttpStatusCode.BadRequest;
                errorCode = ((BadRequestException)ex).ErrorCode;
            }
            else if (ex is ResourceNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                errorCode = ((ResourceNotFoundException) ex).ErrorCode;
            }
            else if (ex is AuthorizationException)
            {
                statusCode = HttpStatusCode.Forbidden;
                errorCode = ((AuthorizationException)ex).ErrorCode;
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;

                var loggableController = context?.ActionContext?.ControllerContext?.Controller;
                var log = GetLog(loggableController);
                if(log != null)
                    await log.LogError(GetLogSource(loggableController), ex);
            }

            var error = new HttpError(ex.Message);
            if (errorCode.HasValue)
                error[ServiceConstants.ErrorCodeName] = errorCode;

            context.Response = context.Request.CreateErrorResponse(statusCode, error);
        }

        private IServiceLogRepository GetLog(IHttpController controller)
        {
            return (controller as ILoggable)?.Log;
        }

        private string GetLogSource(IHttpController controller)
        {
            return (controller as ILoggable)?.LogSource ?? UnknownLogSource;
        }
    }
}
