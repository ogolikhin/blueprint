using System;
using System.Net;
using System.Runtime.Serialization;
using Logging;

namespace Utilities
{
    /**
     * TODO: implement following exceptions:
     * 409: Conflict
     * 411: Length Required
     * 413: Request Entriy Too Large
     * 415: Unsupported Media Type
     * 416: Request Range Not Satisfiable
     * 501: Note Implemented
     **/
    #region Exceptions

    [Serializable]
    public class Http401UnauthorizedException : WebException
    {
        public const string ERROR = "(401) Unauthorized";

        public Http401UnauthorizedException()
        {}

        public Http401UnauthorizedException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        {}

        public Http401UnauthorizedException(string msg)
            : base(msg)
        {}

        public Http401UnauthorizedException(string msg, Exception e)
            : base(msg, e)
        {}

        protected Http401UnauthorizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class Http403BadRequestException : WebException
    {
        public const string ERROR = "(403) Bad Request";

        public Http403BadRequestException()
        {}

        public Http403BadRequestException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        {}

        public Http403BadRequestException(string msg)
            : base(msg)
        {}

        public Http403BadRequestException(string msg, Exception e)
            : base(msg, e)
        {}

        protected Http403BadRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class Http404NotFoundException : WebException
    {
        public const string ERROR = "(404) Not Found";

        public Http404NotFoundException()
        {}

        public Http404NotFoundException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        {}

        public Http404NotFoundException(string msg)
            : base(msg)
        {}

        public Http404NotFoundException(string msg, Exception e)
            : base(msg, e)
        {}

        protected Http404NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class Http406NotAcceptableException : WebException
    {
        public const string ERROR = "(406) Not Acceptable";

        public Http406NotAcceptableException()
        {}

        public Http406NotAcceptableException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        {}

        public Http406NotAcceptableException(string msg)
            : base(msg)
        {}

        public Http406NotAcceptableException(string msg, Exception e)
            : base(msg, e)
        {}

        protected Http406NotAcceptableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class Http500InternalServerErrorException : WebException
    {
        public const string ERROR = "(500) Internal Server Error";

        public Http500InternalServerErrorException()
        {}

        public Http500InternalServerErrorException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        {}

        public Http500InternalServerErrorException(string msg)
            : base(msg)
        {}

        public Http500InternalServerErrorException(string msg, Exception e)
            : base(msg, e)
        {}

        protected Http500InternalServerErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class Http503ServiceUnavailableException : WebException
    {
        public const string ERROR = "(503) Service Unavailable";

        public Http503ServiceUnavailableException()
        {}

        public Http503ServiceUnavailableException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        {}

        public Http503ServiceUnavailableException(string msg)
            : base(msg)
        {}

        public Http503ServiceUnavailableException(string msg, Exception e)
            : base(msg, e)
        {}

        protected Http503ServiceUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class OperationTimedOutException : WebException
    {
        public const string ERROR = "The operation has timed out";

        public OperationTimedOutException()
        { }

        public OperationTimedOutException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        { }

        public OperationTimedOutException(string msg)
            : base(msg)
        { }

        public OperationTimedOutException(string msg, Exception e)
            : base(msg, e)
        { }

        protected OperationTimedOutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class ConnectionClosedException : WebException
    {
        public const string ERROR = "The underlying connection was closed: A pipeline failure occurred";

        public ConnectionClosedException()
        { }

        public ConnectionClosedException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        { }

        public ConnectionClosedException(string msg)
            : base(msg)
        { }

        public ConnectionClosedException(string msg, Exception e)
            : base(msg, e)
        { }

        protected ConnectionClosedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class RequestAbortedException : WebException
    {
        public const string ERROR = "The request was aborted: The request was canceled.";

        public RequestAbortedException()
        { }

        public RequestAbortedException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        { }

        public RequestAbortedException(string msg)
            : base(msg)
        { }

        public RequestAbortedException(string msg, Exception e)
            : base(msg, e)
        { }

        protected RequestAbortedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    #endregion Exceptions

    /// <summary>
    /// Class to convert generic WebExceptions into specific exceptions.
    /// </summary>
    public static class WebExceptionConverter
    {
        /// <summary>
        /// Converts a generic WebException into a specific exception based on the exception message.
        /// </summary>
        /// <param name="ex">The WebException to convert.</param>
        /// <returns>A specific exception derived from WebException, or the same WebException that was passed in if it couldn't parse the exception message.</returns>
        /// <exception cref="ArgumentNullException">If a null argument was passed in.</exception>
        public static WebException Convert(WebException ex)
        {
            if (ex == null) { throw new ArgumentNullException ("ex"); }

            if (ex.Message.Contains(Http401UnauthorizedException.ERROR))
            {
                ex = new Http401UnauthorizedException(ex);
            }
            else if (ex.Message.Contains(Http403BadRequestException.ERROR))
            {
                ex = new Http403BadRequestException(ex);
            }
            else if (ex.Message.Contains(Http404NotFoundException.ERROR))
            {
                ex = new Http404NotFoundException(ex);
            }
            else if (ex.Message.Contains(Http406NotAcceptableException.ERROR))
            {
                ex = new Http406NotAcceptableException(ex);
            }
            else if (ex.Message.Contains(Http500InternalServerErrorException.ERROR))
            {
                ex = new Http500InternalServerErrorException(ex);
            }
            else if (ex.Message.Contains(Http503ServiceUnavailableException.ERROR))
            {
                ex = new Http503ServiceUnavailableException(ex);
            }
            else if (ex.Message.Contains(OperationTimedOutException.ERROR))
            {
                ex = new OperationTimedOutException(ex);
            }
            else if (ex.Message.Contains(ConnectionClosedException.ERROR))
            {
                ex = new ConnectionClosedException(ex);
            }
            else if (ex.Message.Contains(RequestAbortedException.ERROR))
            {
                ex = new RequestAbortedException(ex);
            }
            else
            {
                Logger.WriteWarning("Could not convert the WebException into a specific Http exception.  The WebException message is '{0}'.", ex.Message);
            }

            return ex;
        }
    }
}
