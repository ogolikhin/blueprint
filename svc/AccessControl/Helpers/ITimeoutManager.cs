﻿using System;
using System.Timers;

namespace AccessControl.Helpers
{
    public interface ITimeoutManager<in T> : IDisposable
    {
        void Insert(T item, DateTime timeout, Action callback);
        void Remove(T item);
    }

    /// <summary>
    /// An interface for a timer to facilitate unit testing.
    /// </summary>
    public interface ITimer : IDisposable
    {
        DateTime Now();
        bool AutoReset { get; set; }
        bool Enabled { get; set; }
        double Interval { get; set; }
        event ElapsedEventHandler Elapsed;
    }
}
