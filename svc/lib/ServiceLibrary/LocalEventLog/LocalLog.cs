using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.LocalEventLog
{
    public class LocalLog : ILocalLog
    {
        public static LocalLog Log = new LocalLog();

        private const string LOGNAME = "BlueprintSys";
        private const string LOGSOURCE = "BlueprintSys-Admin";

        private void CreateLogs()
        {
            if (!EventLog.SourceExists(LOGSOURCE))
            {
                EventLog.CreateEventSource(LOGSOURCE, LOGNAME);
            }
        }

        public void LogError(string message)
        {
            CreateLogs();
            EventLog.WriteEntry(LOGSOURCE, message, EventLogEntryType.Error);
        }

        public void LogWarning(string message)
        {
            CreateLogs();
            EventLog.WriteEntry(LOGSOURCE, message, EventLogEntryType.Warning);
        }
    }
}
