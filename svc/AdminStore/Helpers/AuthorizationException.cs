using System;
using System.Runtime.Serialization;

namespace AdminStore.Helpers
{
    [Serializable]
    public class AuthorizationException : Exception
    {
        public int ErrorCode { get; set; }

        public AuthorizationException() : base()
        {
        }

        public AuthorizationException(string message) : base(message)
        {
        }

        public AuthorizationException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(AdminStoreConstants.ErrorCodeName, ErrorCode);
        }
    }
}