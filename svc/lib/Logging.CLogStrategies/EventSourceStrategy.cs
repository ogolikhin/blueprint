using DanielVaughan.Logging;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace Logging.CLogStrategies
{
    public class EventSourceStrategy : ILogStrategy
    {
        internal static IServiceLogRepository _log;

        public EventSourceStrategy() : this(new ServiceLogRepository())
        {
        }

        internal EventSourceStrategy(IServiceLogRepository log)
        {
            _log = log;
        }

        public LogLevel GetLogLevel(IClientInfo clientInfo)
        {
            return LogLevel.All;
        }

        public void Write(IServerLogEntry logEntry)
        {
            var logInfo = new CLogEntry();
            logInfo.Source = "CLog.Server";
            logInfo.LogLevel = GetEventSourceLevel(logEntry.LogLevel);
            logInfo.Message = logEntry.Message;
            logInfo.OccuredAt = logEntry.OccuredAt;

            logInfo.TimeZoneOffset = logEntry.Properties["timeZoneOffset"] == null ? null : logEntry.Properties["timeZoneOffset"].ToString();
            logInfo.UserName = logEntry.Properties["userName"] == null ? null : logEntry.Properties["userName"].ToString();

            // if there is an actionName set this is a profiling event
            if (logEntry.Properties.ContainsKey("actionName"))
            {
                logInfo.ActionName = logEntry.Properties["actionName"] == null ? null : logEntry.Properties["actionName"].ToString();
                double totalDuration = 0;
                double.TryParse(logEntry.Properties["totalDuration"].ToString(), out totalDuration);
                logInfo.TotalDuration = totalDuration;
            }

            logInfo.StackTrace = LogHelper.GetStackTrace(logEntry.Exception);

            _log.LogCLog(logInfo);
        }

        public void Write(IClientLogEntry logEntry)
        {
            var logInfo = new CLogEntry();
            logInfo.Source = "CLog.Client";
            logInfo.LogLevel = GetEventSourceLevel(logEntry.LogLevel);
            logInfo.Message = logEntry.Message;
            logInfo.OccuredAt = logEntry.OccuredAt;

            logInfo.TimeZoneOffset = logEntry.Properties["timeZoneOffset"] == null ? null : logEntry.Properties["timeZoneOffset"].ToString();
            logInfo.UserName = logEntry.Properties["userName"] == null ? null : logEntry.Properties["userName"].ToString();

            // if there is an actionName set this is a profiling event
            if (logEntry.Properties.ContainsKey("actionName"))
            {
                logInfo.ActionName = logEntry.Properties["actionName"] == null ? null : logEntry.Properties["actionName"].ToString();
                double totalDuration = 0;
                double.TryParse(logEntry.Properties["totalDuration"].ToString(), out totalDuration);
                logInfo.TotalDuration = totalDuration;
            }

            if (logEntry.ExceptionMemento != null)
            {   /* Use the exception memento to write 
				 * the message and stack trace etc. */
                logInfo.StackTrace = logEntry.ExceptionMemento.ToString();
            }

            _log.LogCLog(logInfo);
        }

        /// <summary>
        /// Converts a Clog LogLevel to the event source level equivalent.
        /// </summary>
        /// <param name="level">The level to convert.</param>
        /// <returns>The event source level.</returns>
        static LogLevelEnum GetEventSourceLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return LogLevelEnum.Verbose;
                case LogLevel.Info:
                    return LogLevelEnum.Informational;
                case LogLevel.Warn:
                    return LogLevelEnum.Warning;
                case LogLevel.Error:
                    return LogLevelEnum.Error;
                case LogLevel.Fatal:
                    return LogLevelEnum.Critical;
                default:
                    return LogLevelEnum.Verbose;
            }
        }

    }
}
