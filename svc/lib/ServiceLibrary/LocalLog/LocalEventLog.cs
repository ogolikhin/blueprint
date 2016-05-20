// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************
using System.Diagnostics;
using ServiceLibrary.Helpers;

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
            if (IsTest) return;

            if (!EventLog.SourceExists(LOGSOURCE))
            {
                EventLog.CreateEventSource(LOGSOURCE, LOGNAME);
            }
        }

        public bool IsTest { get; set; }

        public void LogError(string message)
        {
            WriteMessage(message, EventLogEntryType.Error);
        }

        public void LogErrorFormat(string format, params object[] args)
        {
            LogError(I18NHelper.FormatInvariant(format, args));
        }

        public void LogWarning(string message)
        {
            WriteMessage(message, EventLogEntryType.Warning);
        }

        public void LogWarningFormat(string format, params object[] args)
        {
            LogWarning(I18NHelper.FormatInvariant(format, args));
        }

        public void LogInformation(string message)
        {
            WriteMessage(message, EventLogEntryType.Information);
        }

        public void LogInformationFormat(string format, params object[] args)
        {
            LogInformation(I18NHelper.FormatInvariant(format, args));
        }

        private void WriteMessage(string message, EventLogEntryType type)
        {
            if (IsTest) return;

            EventLog.WriteEntry(LOGSOURCE, message, type);
        }
    }
}
