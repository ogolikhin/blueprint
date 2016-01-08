using System;
using System.Diagnostics.Tracing;

namespace ServiceLibrary.EventSources
{
    [EventSource(Name = "BlueprintSys-Blueprint-CLog")]
    public sealed class CLogEventSource : EventSource
    {
        public static CLogEventSource Log = new CLogEventSource();

        #region Standard Events

        [Event(100, Level = EventLevel.Critical, Message = "{2}")]
        public void Critical(string IpAddress, string Source, string Message, string StackTrace, DateTime OccuredAt, string TimeZoneOffset, string UserName)
        {
            WriteEvent(100, IpAddress, Source, Message, StackTrace, OccuredAt, TimeZoneOffset, UserName);
        }

        [Event(200, Level = EventLevel.Error, Message = "{2}")]
        public void Error(string IpAddress, string Source, string Message, string StackTrace, DateTime OccuredAt, string TimeZoneOffset, string UserName)
        {
            WriteEvent(200, IpAddress, Source, Message, StackTrace, OccuredAt, TimeZoneOffset, UserName);
        }

        [Event(300, Level = EventLevel.Warning, Message = "{2}")]
        public void Warning(string IpAddress, string Source, string Message, DateTime OccuredAt, string TimeZoneOffset, string UserName)
        {
            WriteEvent(300, IpAddress, Source, Message, OccuredAt, TimeZoneOffset, UserName);
        }

        [Event(400, Level = EventLevel.Informational, Message = "{2}")]
        public void Informational(string IpAddress, string Source, string Message, DateTime OccuredAt, string TimeZoneOffset, string UserName)
        {
            WriteEvent(400, IpAddress, Source, Message, OccuredAt, TimeZoneOffset, UserName);
        }

        [Event(500, Level = EventLevel.Verbose, Message = "{2}")]
        public void Verbose(string IpAddress, string Source, string Message, DateTime OccuredAt, string TimeZoneOffset, string UserName)
        {
            WriteEvent(500, IpAddress, Source, Message, OccuredAt, TimeZoneOffset, UserName);
        }

        #endregion

        #region Performance Events

        [Event(101, Level = EventLevel.Critical, Message = "{7}, [PROFILING], {8}, {2}")]
        public void CriticalPerf(string IpAddress, string Source, string Message, string StackTrace, DateTime OccuredAt, string TimeZoneOffset, string UserName, string ActionName, double TotalDuration)
        {
            WriteEvent(101, IpAddress, Source, Message, StackTrace, OccuredAt, TimeZoneOffset, UserName, ActionName, TotalDuration);
        }

        [Event(201, Level = EventLevel.Error, Message = "{7}, [PROFILING], {8}, {2}")]
        public void ErrorPerf(string IpAddress, string Source, string Message, string StackTrace, DateTime OccuredAt, string TimeZoneOffset, string UserName, string ActionName, double TotalDuration)
        {
            WriteEvent(201, IpAddress, Source, Message, StackTrace, OccuredAt, TimeZoneOffset, UserName, ActionName, TotalDuration);
        }

        [Event(301, Level = EventLevel.Warning, Message = "{6}, [PROFILING], {7}, {2}")]
        public void WarningPerf(string IpAddress, string Source, string Message, DateTime OccuredAt, string TimeZoneOffset, string UserName, string ActionName, double TotalDuration)
        {
            WriteEvent(301, IpAddress, Source, Message, OccuredAt, TimeZoneOffset, UserName, ActionName, TotalDuration);
        }

        [Event(401, Level = EventLevel.Informational, Message = "{6}, [PROFILING], {7}, {2}")]
        public void InformationalPerf(string IpAddress, string Source, string Message, DateTime OccuredAt, string TimeZoneOffset, string UserName, string ActionName, double TotalDuration)
        {
            WriteEvent(401, IpAddress, Source, Message, OccuredAt, TimeZoneOffset, UserName, ActionName, TotalDuration);
        }

        [Event(501, Level = EventLevel.Verbose, Message = "{6}, [PROFILING], {7}, {2}")]
        public void VerbosePerf(string IpAddress, string Source, string Message, DateTime OccuredAt, string TimeZoneOffset, string UserName, string ActionName, double TotalDuration)
        {
            WriteEvent(501, IpAddress, Source, Message, OccuredAt, TimeZoneOffset, UserName, ActionName, TotalDuration);
        }

        #endregion

    }
}