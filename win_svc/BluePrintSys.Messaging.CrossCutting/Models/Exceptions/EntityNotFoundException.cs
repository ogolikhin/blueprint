using System;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace BluePrintSys.Messaging.CrossCutting.Models.Exceptions
{
    [Serializable]
    public class EntityNotFoundException : ExceptionWithErrorCode
    {
        public EntityNotFoundException(string message) : base(message, ErrorCodes.ItemNotFound)
        {
        }
    }
}
