using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class ElasticsearchConfigurationException : ExceptionWithErrorCode
    {
        public ElasticsearchConfigurationException(string message) : base(message, ErrorCodes.ElasticsearchConfigurationError)
        {

        }
    }
}