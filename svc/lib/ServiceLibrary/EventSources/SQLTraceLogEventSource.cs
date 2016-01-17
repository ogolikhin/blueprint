using System;
using System.Diagnostics.Tracing;

namespace ServiceLibrary.EventSources
{
    [EventSource(Name = "BlueprintSys-Blueprint-SQLTraceLog")]
    public sealed class SQLTraceLogEventSource : EventSource
    {
        public static SQLTraceLogEventSource Log = new SQLTraceLogEventSource();

        #region Performance Events

        [Event(500, Level = EventLevel.Verbose, Message = "{7}, [SQLTRACE], {9}, {8}, {2}, {14}, {15}")]
        public void Verbose(string IpAddress, string Source, string Message, DateTime OccuredAt, string SessionId, string UserName, string ThreadId, string ActionName, Guid CorrelationId, double Duration, string Namespace, string Class, string Test, string TextData, int SPID, string Database)
        {
            WriteEvent(500, IpAddress, Source, Message, OccuredAt, SessionId, UserName, ThreadId, ActionName, CorrelationId, Duration, Namespace, Class, Test, TextData, SPID, Database);
        }

        #endregion

    }
}