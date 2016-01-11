using System;
using System.Diagnostics.Tracing;

namespace ServiceLibrary.EventSources
{
    [EventSource(Name = "BlueprintSys-Blueprint-StandardLog")]
    public sealed class StandardLogEventSource : EventSource
    {
        public static StandardLogEventSource Log = new StandardLogEventSource();

        #region Standard Events

        [Event(100, Level = EventLevel.Critical, Message = "{2}")]
        public void Critical(string IpAddress, string Source, string Message, string StackTrace, DateTime DateTime, string SessionId, string UserName, string TimeZoneOffset, string ThreadId)
        {
            WriteEvent(100, IpAddress, Source, Message, StackTrace, DateTime, SessionId, UserName, TimeZoneOffset, ThreadId);
        }

        [Event(200, Level = EventLevel.Error, Message = "{2}")]
        public void Error(string IpAddress, string Source, string Message, string StackTrace, DateTime DateTime, string SessionId, string UserName, string TimeZoneOffset, string ThreadId)
        {
            WriteEvent(200, IpAddress, Source, Message, StackTrace, DateTime, SessionId, UserName, TimeZoneOffset, ThreadId);
        }

        [Event(300, Level = EventLevel.Warning, Message = "{2}")]
        public void Warning(string IpAddress, string Source, string Message, DateTime DateTime, string SessionId, string UserName, string TimeZoneOffset, string ThreadId)
        {
            WriteEvent(300, IpAddress, Source, Message, DateTime, SessionId, UserName, TimeZoneOffset, ThreadId);
        }

        [Event(400, Level = EventLevel.Informational, Message = "{2}")]
        public void Informational(string IpAddress, string Source, string Message, DateTime DateTime, string SessionId, string UserName, string TimeZoneOffset, string ThreadId)
        {
            WriteEvent(400, IpAddress, Source, Message, DateTime, SessionId, UserName, TimeZoneOffset, ThreadId);
        }

        [Event(500, Level = EventLevel.Verbose, Message = "{2}")]
        public void Verbose(string IpAddress, string Source, string Message, DateTime DateTime, string SessionId, string UserName, string TimeZoneOffset, string ThreadId)
        {
            WriteEvent(500, IpAddress, Source, Message, DateTime, SessionId, UserName, TimeZoneOffset, ThreadId);
        }

        #endregion

    }
}