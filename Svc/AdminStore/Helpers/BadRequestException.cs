using System;
using System.Runtime.Serialization;

namespace AdminStore.Helpers
{
    [Serializable]
    public class BadRequestException : Exception
    {
        public int ErrorCode { get; set; }

        public BadRequestException() : base()
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(string message, int errorCode) : base(message)
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