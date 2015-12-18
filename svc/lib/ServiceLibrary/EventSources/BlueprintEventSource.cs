using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Web;

namespace ServiceLibrary.EventSources
{
    [EventSource(Name = "BlueprintSys-Blueprint-Blueprint")]
    public sealed class BlueprintEventSource : EventSource
    {
        public static BlueprintEventSource Log = new BlueprintEventSource();

        [Event(100, Level = EventLevel.Error, Message = "{2}")]
        public void Error(string ipAddress, string source, string message, string methodName, string filePath, int lineNumber, string stackTrace)
        {
            if (IsEnabled(EventLevel.Error, EventKeywords.None))
            {
                WriteEvent(100, ipAddress, source, message, methodName, filePath, lineNumber, stackTrace);
            }
        }

        [Event(200, Level = EventLevel.Warning, Message = "{2}")]
        public void Warning(string ipAddress, string source, string message, string methodName, string filePath, int lineNumber, string stackTrace)
        {
            if (IsEnabled(EventLevel.Warning, EventKeywords.None))
            {
                WriteEvent(200, ipAddress, source, message, methodName, filePath, lineNumber, stackTrace);
            }
        }

        [Event(300, Level = EventLevel.Informational, Message = "{2}")]
        public void Informational(string ipAddress, string source, string message, string methodName, string filePath, int lineNumber, string stackTrace)
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                WriteEvent(300, ipAddress, source, message, methodName, filePath, lineNumber, stackTrace);
            }
        }

        [Event(400, Level = EventLevel.Verbose, Message = "{2}")]
        public void Verbose(string ipAddress, string source, string message, string methodName, string filePath, int lineNumber, string stackTrace)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                WriteEvent(400, ipAddress, source, message, methodName, filePath, lineNumber, stackTrace);
            }
        }

    }
}