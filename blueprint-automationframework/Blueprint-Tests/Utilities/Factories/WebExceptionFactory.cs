using System.Net;
using Logging;

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
        /// <returns>A WebException or a child of WebException appropriate for the specified status code.</returns>
        public static WebException Create(int statusCode)
        {
            WebException ex = null;
            string message = string.Format("Received status code: {0}.", statusCode);

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
                case 500:
                    ex = new Http500InternalServerErrorException(message);
                    break;
                case 503:
                    ex = new Http503ServiceUnavailableException(message);
                    break;
                default:
                    return new WebException(string.Format("Unrecognized status code {0} recieved!", statusCode));
            }

            Logger.WriteDebug(message);
            return ex;
        }
    }
}
