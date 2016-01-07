using System.Diagnostics.Tracing;

namespace ServiceLibrary.EventSources
{
    [EventSource(Name = "BlueprintSys-Blueprint-Blueprint")]
    public sealed class BlueprintEventSource : EventSource
    {
        public static BlueprintEventSource Log = new BlueprintEventSource();

        #region Standard Events

        [Event(100, Level = EventLevel.Critical, Message = "{2}")]
        public void Critical(string IpAddress, string Source, string Message, string MethodName, string FilePath, int LineNumber, string StackTrace)
        {
            if (IsEnabled(EventLevel.Critical, EventKeywords.None))
            {
                WriteEvent(100, IpAddress, Source, Message, MethodName, FilePath, LineNumber, StackTrace);
            }
        }

        [Event(200, Level = EventLevel.Error, Message = "{2}")]
        public void Error(string IpAddress, string Source, string Message, string MethodName, string FilePath, int LineNumber, string StackTrace)
        {
            if (IsEnabled(EventLevel.Error, EventKeywords.None))
            {
                WriteEvent(200, IpAddress, Source, Message, MethodName, FilePath, LineNumber, StackTrace);
            }
        }

        [Event(300, Level = EventLevel.Warning, Message = "{2}")]
        public void Warning(string IpAddress, string Source, string Message, string MethodName, string FilePath, int LineNumber)
        {
            if (IsEnabled(EventLevel.Warning, EventKeywords.None))
            {
                WriteEvent(300, IpAddress, Source, Message, MethodName, FilePath, LineNumber);
            }
        }

        [Event(400, Level = EventLevel.Informational, Message = "{2}")]
        public void Informational(string IpAddress, string Source, string Message, string MethodName, string FilePath, int LineNumber)
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                WriteEvent(400, IpAddress, Source, Message, MethodName, FilePath, LineNumber);
            }
        }

        [Event(500, Level = EventLevel.Verbose, Message = "{2}")]
        public void Verbose(string IpAddress, string Source, string Message, string MethodName, string FilePath, int LineNumber)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                WriteEvent(500, IpAddress, Source, Message, MethodName, FilePath, LineNumber);
            }
        }

        #endregion

    }
}