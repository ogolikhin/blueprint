using System;
using System.Diagnostics;

namespace ServiceLibrary.Log
{
    public class EventLogProviderImpl : ILogProvider
    {
        public EventLogProviderImpl(string source, string logName)
        {
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, logName);
            }
        }

        public void WriteEntry(string source, string message, LogEntryType logType)
        {
            try
            {
                EventLogEntryType eventLogEntryType;
                Enum.TryParse(logType.ToString(), out eventLogEntryType);
                EventLog.WriteEntry(source, message, eventLogEntryType);
            }
            catch (ArgumentException)
            {

            }
        }
    }
}
