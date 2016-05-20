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

        public ExceptionWithErrorCode() : base()
        {
        }

        public ExceptionWithErrorCode(string message) : base(message)
        {
        }

        public ExceptionWithErrorCode(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
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
