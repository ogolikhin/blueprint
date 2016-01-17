using System;
using System.Diagnostics.Tracing;

namespace ServiceLibrary.EventSources
{
    [EventSource(Name = "BlueprintSys-Blueprint-PerformanceLog")]
    public sealed class PerformanceLogEventSource : EventSource
    {
        public static PerformanceLogEventSource Log = new PerformanceLogEventSource();

        #region Performance Events

        [Event(500, Level = EventLevel.Verbose, Message = "{7}, [PROFILING], {9}, {8}, {2}")]
        public void Verbose(string IpAddress, string Source, string Message, DateTime OccuredAt, string SessionId, string UserName, string ThreadId, string ActionName, Guid CorrelationId, double Duration, string Namespace, string Class, string Test)
        {
            WriteEvent(500, IpAddress, Source, Message, OccuredAt, SessionId, UserName, ThreadId, ActionName, CorrelationId, Duration, Namespace, Class, Test);
        }

        #endregion

    }
}