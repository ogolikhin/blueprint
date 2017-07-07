using System;
using System.Runtime.Serialization;
using System.Web.Http;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class ExceptionWithErrorCode : Exception
    {
        public int ErrorCode { get; set; }
        public readonly object Content;

        public ExceptionWithErrorCode() : this(null)
        {
        }

        public ExceptionWithErrorCode(string message) : this(message, 0)
        {
        }

        public ExceptionWithErrorCode(string message, int errorCode) : this(message, errorCode, null)
        {
        }

        public ExceptionWithErrorCode(string message, int errorCode, object content) : base(message)
        {
            ErrorCode = errorCode;
            Content = content;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(ServiceConstants.ErrorCodeName, ErrorCode);
        }

        public HttpError CreateHttpError()
        {
            return new HttpError(Message) { { ServiceConstants.ErrorCodeName, ErrorCode } };
        }
    }
}
