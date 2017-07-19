using System;
using System.Collections.Generic;
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

        private Dictionary<Type, HttpStatusCode> HttpStatusCodesByExceptionType = new Dictionary<Type, HttpStatusCode>
        {
            { typeof(AuthenticationException), HttpStatusCode.Unauthorized },
            { typeof(NotImplementedException), HttpStatusCode.NotImplemented },
            { typeof(BadRequestException), HttpStatusCode.BadRequest },
            { typeof(ResourceNotFoundException), HttpStatusCode.NotFound },
            { typeof(AuthorizationException), HttpStatusCode.Forbidden },
            { typeof(SqlTimeoutException), HttpStatusCode.ServiceUnavailable },
            { typeof(ConflictException), HttpStatusCode.Conflict }
        };

        public override async Task OnExceptionAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            HttpStatusCode statusCode;
            if (!HttpStatusCodesByExceptionType.TryGetValue(context.Exception.GetType(), out statusCode))
            {
                statusCode = HttpStatusCode.InternalServerError;
            }

            int? errorCode = null;
            object errorContent = null;
            var exceptionWithErrorCode = context.Exception as ExceptionWithErrorCode;
            if (exceptionWithErrorCode != null)
            {
                errorCode = exceptionWithErrorCode.ErrorCode;
                errorContent = exceptionWithErrorCode.Content;
            }
            
            
            var loggableController = context.ActionContext?.ControllerContext?.Controller;
            var log = GetLog(loggableController);

            if (log != null)
            {
                await log.LogError(GetLogSource(loggableController), context.Exception);
            }

            var error = new HttpError(context.Exception.Message);
            if (errorCode.HasValue)
            {
                error[ServiceConstants.ErrorCodeName] = errorCode;
            }
            if (errorContent != null)
            {
                error[ServiceConstants.ErrorContentName] = errorContent;
            }

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
