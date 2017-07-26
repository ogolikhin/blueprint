using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ActionHandlerService.Models.Exceptions
{
    public class TenantInfoNotFoundException : ExceptionWithErrorCode
    {
        public TenantInfoNotFoundException(string message) : base(message, ErrorCodes.TenantInfoNotFound)
        {
        }
    }

    public class UnsupportedActionTypeException : ExceptionWithErrorCode
    {
        public UnsupportedActionTypeException(string message) : base(message, ErrorCodes.UnsupportedActionType)
        {
        }
    }

    public class MessageHeaderValueNotFoundException : ExceptionWithErrorCode
    {
        public MessageHeaderValueNotFoundException(string message) : base(message, ErrorCodes.MessageHeaderValueNotFound)
        {
        }
    }
}
