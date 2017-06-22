﻿using System;

namespace ServiceLibrary.Helpers
{
    public interface ITimeProvider
    {
        DateTime CurrentDateTime { get; }
        DateTime CurrentUniversalTime { get; }
    }

    public class TimeProvider : ITimeProvider
    {
        public DateTime CurrentDateTime => DateTime.Now;
        public DateTime CurrentUniversalTime => DateTime.Now.ToUniversalTime();
    }
}
