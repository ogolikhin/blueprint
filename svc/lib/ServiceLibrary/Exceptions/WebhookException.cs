using System;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class WebhookExceptionDoNotRetry : ExceptionWithErrorCode
    {
        public WebhookExceptionDoNotRetry(string message) : base(message)
        {
        }

        public WebhookExceptionDoNotRetry(string message, int errorCode) : base(message, errorCode)
        {
        }
    }

    [Serializable]
    public class WebhookExceptionRetryPerPolicy : ExceptionWithErrorCode
    {
        public WebhookExceptionRetryPerPolicy(string message) : base(message)
        {
        }

        public WebhookExceptionRetryPerPolicy(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
