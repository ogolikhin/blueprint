using System;
using System.Runtime.Serialization;

namespace AdminStore.Helpers
{
    [Serializable]
    public class ResourceNotFoundException : Exception
    {
        public int ErrorCode { get; set; }

        public ResourceNotFoundException() : base()
        {
        }

        public ResourceNotFoundException(string message) : base(message)
        {
        }

        public ResourceNotFoundException(string message, int errorCode) : base(message)
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
