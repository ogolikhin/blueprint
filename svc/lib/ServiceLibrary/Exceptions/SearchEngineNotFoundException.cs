using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class SearchEngineNotFoundException: ExceptionWithErrorCode
    {
        public SearchEngineNotFoundException(string message) : base(message, ErrorCodes.SearchEngineNotFound)
        {
            
        }
    }
}
