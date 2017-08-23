using System;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace BluePrintSys.Messaging.CrossCutting.Models.Exceptions
{
    [Serializable]
    public class UnsupportedActionTypeException : ExceptionWithErrorCode
    {
        public UnsupportedActionTypeException(string message) : base(message, ErrorCodes.UnsupportedActionType)
        {
        }
    }
}
