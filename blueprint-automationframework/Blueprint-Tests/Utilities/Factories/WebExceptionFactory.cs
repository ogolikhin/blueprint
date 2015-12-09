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
        /// <param name="statusCode">The HTTP status code (must be a number as a string).</param>
        /// <returns>A WebException or a child of WebException appropriate for the specified status code.</returns>
        public static WebException Create(string statusCode)
        {
            WebException ex = null;
            string message = string.Format("Received status code: {0}.", statusCode);

            switch (statusCode)
            {
                case "401":
                    ex = new Http401UnauthorizedException(message);
                    break;
                case "403":
                    ex = new Http403BadRequestException(message);
                    break;
                case "404":
                case "NotFound":
                    ex = new Http404NotFoundException(message);
                    break;
                case "406":
                    ex = new Http406NotAcceptableException(message);
                    break;
                case "500":
                    ex = new Http500InternalServerErrorException(message);
                    break;
                case "503":
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
