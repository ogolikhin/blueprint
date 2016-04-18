﻿using System;
using System.Runtime.Serialization;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class ResourceNotFoundException : ExceptionWithErrorCode
    {
        public ResourceNotFoundException() : base()
        {
        }

        public ResourceNotFoundException(string message) : base(message)
        {
        }

        public ResourceNotFoundException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
