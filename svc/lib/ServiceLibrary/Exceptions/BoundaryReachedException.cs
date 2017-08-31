using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class BoundaryReachedException : ExceptionWithErrorCode
    {
        public BoundaryReachedException(string message) : base(message, ErrorCodes.BoundaryReached)
        {
        }
    }
}
