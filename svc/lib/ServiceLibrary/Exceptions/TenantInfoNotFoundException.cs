using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class TenantInfoNotFoundException : ExceptionWithErrorCode
    {
        public TenantInfoNotFoundException(string message) : base(message, ErrorCodes.TenantInfoNotFound)
        {
        }
    }
}
