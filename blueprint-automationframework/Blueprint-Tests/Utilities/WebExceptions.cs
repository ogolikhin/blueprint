using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Common;
using Utilities.Facades;


namespace Utilities
{
    /**
     * TODO: implement following exceptions:
     * 411: Length Required
     * 413: Request Entriy Too Large
     * 415: Unsupported Media Type
     * 416: Request Range Not Satisfiable
     * 501: Not Implemented
     **/
    #region Exceptions

    [Serializable]
    public class HttpRequestBaseException : WebException
    {
        public Facades.RestResponse RestResponse { get; set; }
        
        public HttpRequestBaseException()
        { }

        public HttpRequestBaseException(WebException ex)
            : base(ex?.Message, ex)
        { }

        public HttpRequestBaseException(string msg)
            : base(msg)
        { }

        public HttpRequestBaseException(string msg, Exception e)
            : base(msg, e)
        { }

        protected HttpRequestBaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        #region ISerializable members

        /// <seealso cref="ISerializable.GetObjectData(SerializationInfo, StreamingContext)" />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ThrowIf.ArgumentNull(info, nameof(info));

            base.GetObjectData(info, context);
        }

        #endregion ISerializable members
    }

    [Serializable]
    public class Http400BadRequestException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 400";

        public Http400BadRequestException()
        { }

        public Http400BadRequestException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        { }

        public Http400BadRequestException(string msg)
            : base(msg)
        { }

        public Http400BadRequestException(string msg, Exception e)
            : base(msg, e)
        { }

        protected Http400BadRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class Http401UnauthorizedException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 401";

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
    public class Http403ForbiddenException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 403";

        public Http403ForbiddenException()
        {}

        public Http403ForbiddenException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        {}

        public Http403ForbiddenException(string msg)
            : base(msg)
        {}

        public Http403ForbiddenException(string msg, Exception e)
            : base(msg, e)
        {}

        protected Http403ForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    [Serializable]
    public class Http404NotFoundException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 404";

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
    public class Http405MethodNotAllowedException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 405.";

        public Http405MethodNotAllowedException()
        { }

        public Http405MethodNotAllowedException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        { }

        public Http405MethodNotAllowedException(string msg)
            : base(msg)
        { }

        public Http405MethodNotAllowedException(string msg, Exception e)
            : base(msg, e)
        { }

        protected Http405MethodNotAllowedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class Http406NotAcceptableException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 406";

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
    public class Http409ConflictException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 409";

        public Http409ConflictException()
        { }

        public Http409ConflictException(WebException ex)
            : base(((ex == null) ? ERROR : ex.Message), ex)
        { }

        public Http409ConflictException(string msg)
            : base(msg)
        { }

        public Http409ConflictException(string msg, Exception e)
            : base(msg, e)
        { }

        protected Http409ConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class Http500InternalServerErrorException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 500";

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
    public class Http503ServiceUnavailableException : HttpRequestBaseException
    {
        public const string ERROR = "Received status code: 503";

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
    public class OperationTimedOutException : HttpRequestBaseException
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
    public class ConnectionClosedException : HttpRequestBaseException
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
    public class RequestAbortedException : HttpRequestBaseException
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
        /// <param name="restResponse">(optional) The REST response info received that caused this exception.</param>
        /// <returns>A specific exception derived from HttpRequestExceptionBase, or the same WebException that was passed in if it couldn't parse the exception message.</returns>
        /// <exception cref="ArgumentNullException">If a null argument was passed in.</exception>
        public static HttpRequestBaseException Convert(WebException ex, RestResponse restResponse = null)
        {
            ThrowIf.ArgumentNull(ex, nameof(ex));
            HttpRequestBaseException newEx = null;

            if (ex.Message.Contains(Http400BadRequestException.ERROR))
            {
                newEx = new Http400BadRequestException(ex);
            }
            else if (ex.Message.Contains(Http401UnauthorizedException.ERROR))
            {
                newEx = new Http401UnauthorizedException(ex);
            }
            else if (ex.Message.Contains(Http403ForbiddenException.ERROR))
            {
                newEx = new Http403ForbiddenException(ex);
            }
            else if (ex.Message.Contains(Http404NotFoundException.ERROR))
            {
                newEx = new Http404NotFoundException(ex);
            }
            else if (ex.Message.Contains(Http405MethodNotAllowedException.ERROR))
            {
                newEx = new Http405MethodNotAllowedException(ex);
            }
            else if (ex.Message.Contains(Http406NotAcceptableException.ERROR))
            {
                newEx = new Http406NotAcceptableException(ex);
            }
            else if (ex.Message.Contains(Http409ConflictException.ERROR))
            {
                newEx = new Http409ConflictException(ex);
            }
            else if (ex.Message.Contains(Http500InternalServerErrorException.ERROR))
            {
                newEx = new Http500InternalServerErrorException(ex);
            }
            else if (ex.Message.Contains(Http503ServiceUnavailableException.ERROR))
            {
                newEx = new Http503ServiceUnavailableException(ex);
            }
            else if (ex.Message.Contains(OperationTimedOutException.ERROR))
            {
                newEx = new OperationTimedOutException(ex);
            }
            else if (ex.Message.Contains(ConnectionClosedException.ERROR))
            {
                newEx = new ConnectionClosedException(ex);
            }
            else if (ex.Message.Contains(RequestAbortedException.ERROR))
            {
                newEx = new RequestAbortedException(ex);
            }
            else
            {
                Logger.WriteWarning("Could not convert the WebException into a specific Http exception.  The WebException message is '{0}'.", ex.Message);
                newEx = new HttpRequestBaseException(ex);
            }

            if (restResponse != null)
            {
                newEx.RestResponse = restResponse;
            }

            return newEx;
        }
    }
}
