using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web.Http;

namespace AdminStore.Helpers
{
    [Serializable]
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message)
            : base(message)
        {
            ErrorCode = 0;
        }

        public AuthenticationException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public int ErrorCode { get; set; }

        public HttpError CreateHttpError()
        {
            return new HttpError(Message) {{"ErrorCode", ErrorCode}};
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", ErrorCode);
        }
    }
}