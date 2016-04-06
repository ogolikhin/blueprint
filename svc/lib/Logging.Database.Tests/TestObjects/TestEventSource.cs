using System;
using System.Diagnostics.Tracing;

namespace Logging.Database.TestObjects
{
    [EventSource(Name = "Test")]
    public sealed class TestEventSource : EventSource
    {
        public static TestEventSource Log = new TestEventSource();

        [Event(400, Level = EventLevel.Informational, Message = "{2}")]
        public void Informational(string IpAddress, string Source, string Message, DateTime OccurredAt, string MethodName, string FilePath, int LineNumber)
        {
            WriteEvent(400, IpAddress, Source, Message, OccurredAt, MethodName, FilePath, LineNumber);
        }

    }
}