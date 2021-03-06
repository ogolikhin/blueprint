﻿using System;
using System.Diagnostics.Tracing;

namespace ServiceLibrary.EventSources
{
    [EventSource(Name = "BlueprintSys-Blueprint-Blueprint")]
    public sealed class BlueprintEventSource : EventSource
    {
        public static BlueprintEventSource Log = new BlueprintEventSource();

        #region Standard Events

        [Event(100, Level = EventLevel.Critical, Message = "{2}")]
        public void Critical(string IpAddress, string Source, string Message, DateTime OccurredAt, string MethodName, string FilePath, int LineNumber, string StackTrace)
        {
            WriteEvent(100, IpAddress, Source, Message, OccurredAt, MethodName, FilePath, LineNumber, StackTrace);
        }

        [Event(200, Level = EventLevel.Error, Message = "{2}")]
        public void Error(string IpAddress, string Source, string Message, DateTime OccurredAt, string MethodName, string FilePath, int LineNumber, string StackTrace)
        {
            WriteEvent(200, IpAddress, Source, Message, OccurredAt, MethodName, FilePath, LineNumber, StackTrace);
        }

        [Event(300, Level = EventLevel.Warning, Message = "{2}")]
        public void Warning(string IpAddress, string Source, string Message, DateTime OccurredAt, string MethodName, string FilePath, int LineNumber)
        {
            WriteEvent(300, IpAddress, Source, Message, OccurredAt, MethodName, FilePath, LineNumber);
        }

        [Event(400, Level = EventLevel.Informational, Message = "{2}")]
        public void Informational(string IpAddress, string Source, string Message, DateTime OccurredAt, string MethodName, string FilePath, int LineNumber)
        {
            WriteEvent(400, IpAddress, Source, Message, OccurredAt, MethodName, FilePath, LineNumber);
        }

        [Event(500, Level = EventLevel.Verbose, Message = "{2}")]
        public void Verbose(string IpAddress, string Source, string Message, DateTime OccurredAt, string MethodName, string FilePath, int LineNumber)
        {
            WriteEvent(500, IpAddress, Source, Message, OccurredAt, MethodName, FilePath, LineNumber);
        }

        #endregion

    }
}