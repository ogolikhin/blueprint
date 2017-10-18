using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class ElasticsearchException : ExceptionWithErrorCode
    {
        public ElasticsearchException(string message) : base(message, ErrorCodes.ElasticsearchQueryError)
        {
            
        }
    }
}