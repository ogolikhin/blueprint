/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************
using System.Diagnostics;

namespace ServiceLibrary.LocalLog
{
    public class LocalEventLog : ILocalLog
    {
        private const string LOGNAME = "BlueprintSys";
        private const string LOGSOURCE = "BlueprintSys-Admin";

        public LocalEventLog()
        {
            CreateLogs();
        }

        private void CreateLogs()
        {
            if (!EventLog.SourceExists(LOGSOURCE))
            {
                EventLog.CreateEventSource(LOGSOURCE, LOGNAME);
            }
        }

        public void LogError(string message)
        {
            EventLog.WriteEntry(LOGSOURCE, message, EventLogEntryType.Error);
        }

        public void LogWarning(string message)
        {
            EventLog.WriteEntry(LOGSOURCE, message, EventLogEntryType.Warning);
        }

        public void LogInformation(string message)
        {
            EventLog.WriteEntry(LOGSOURCE, message, EventLogEntryType.Information);
        }
    }
}
