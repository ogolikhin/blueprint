﻿using System.Diagnostics;
using System.Globalization;

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

        public void LogErrorFormat(string format, params object[] args)
        {
            LogError(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public void LogWarning(string message)
        {
            EventLog.WriteEntry(LOGSOURCE, message, EventLogEntryType.Warning);
        }

        public void LogWarningFormat(string format, params object[] args)
        {
            LogWarning(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public void LogInformation(string message)
        {
            EventLog.WriteEntry(LOGSOURCE, message, EventLogEntryType.Information);
        }

        public void LogInformationFormat(string format, params object[] args)
        {
            LogInformation(string.Format(CultureInfo.InvariantCulture, format, args));
        }
    }
}
