using System;
using Common;
using Utilities.Facades;

namespace Utilities.Factories
{
    /// <summary>
    /// A factory for creating WebException derivitives.
    /// </summary>
    public static class WebExceptionFactory
    {
        /// <summary>
        /// Create the appropriate WebException object based on the specified status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code number.</param>
        /// <param name="innerExceptionMsg">The inner exception message.</param>
        /// <param name="restResponse">(optional) The REST response to include in the exception.</param>
        /// <returns>A HttpRequestBaseException or a child of HttpRequestBaseException appropriate for the specified status code.</returns>
        public static HttpRequestBaseException Create(int statusCode, string innerExceptionMsg, RestResponse restResponse = null)
        {
            if (statusCode < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(statusCode), "You can't have negative status codes!");
            }
            
            HttpRequestBaseException ex = null;
            string message = I18NHelper.FormatInvariant("Received status code: {0}.  Content = '{1}'",
                statusCode, restResponse?.Content ?? "null");

            switch (statusCode)
            {
                case 400:
                    ex = new Http400BadRequestException(message);
                    break;
                case 401:
                    ex = new Http401UnauthorizedException(message);
                    break;
                case 403:
                    ex = new Http403ForbiddenException(message);
                    break;
                case 404:
                    ex = new Http404NotFoundException(message);
                    break;
                case 405:
                    ex = new Http405MethodNotAllowedException(message);
                    break;
                case 406:
                    ex = new Http406NotAcceptableException(message);
                    break;
                case 409:
                    ex = new Http409ConflictException(message);
                    break;
                case 500:
                    ex = new Http500InternalServerErrorException(message);
                    break;
                case 501:
                    ex = new Http501NotImplementedException(message);
                    break;
                case 503:
                    ex = new Http503ServiceUnavailableException(message);
                    break;
                default:
                    if (innerExceptionMsg == null)
                    {
                        innerExceptionMsg = string.Empty;
                    }

                    ex = new HttpRequestBaseException(I18NHelper.FormatInvariant("Unrecognized status code {0} recieved!  Inner Exception = '{1}'", statusCode, innerExceptionMsg));
                    break;
            }

            Logger.WriteDebug(message);
            ex.RestResponse = restResponse;

            return ex;
        }
    }
}
