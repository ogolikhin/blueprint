using System;

namespace FileStore.Helpers
{
    [Serializable]
    public class MultipartReadException : Exception
    {
        public MultipartReadException(string message)
            : base(message)
        {
        }
    }
}