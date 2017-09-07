using System;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace BluePrintSys.Messaging.CrossCutting.Models.Exceptions
{
    [Serializable]
    public class MessageHeaderValueNotFoundException : ExceptionWithErrorCode
    {
        public MessageHeaderValueNotFoundException(string message) : base(message, ErrorCodes.MessageHeaderValueNotFound)
        {
        }
    }
}
