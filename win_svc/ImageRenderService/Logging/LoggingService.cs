using BluePrintSys.RC.CrossCutting.Logging;
using log4net;
using ServiceLibrary.Repositories.ConfigControl;
using SLM = ServiceLibrary.Models;

namespace ImageRenderService.Logging
{

    internal class LoggingService
    {
        /// <summary>
        /// Write the log entry into the logging service
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="message"></param>
        internal static void LogToLoggingService(StandardLogEntry entry, object message)
        {
            var logInfo = new SLM.StandardLogModel();
            logInfo.Source = "Server";
            logInfo.LogLevel = GetEventSourceLevel(entry.Level);
            logInfo.OccurredAt = entry.DateTime;
            logInfo.Message = message.ToString();
            logInfo.SessionId = entry.SessionId;
            logInfo.UserName = entry.UserName;

            IServiceLogRepository log = new ServiceLogRepository();
            log.LogStandardLog(logInfo);
        }
        

        /// <summary>
        /// Converts a Cross Cutting LogLevel to the event source level equivalent.
        /// </summary>
        /// <param name="level">The level to convert.</param>
        /// <returns>The event source level.</returns>
        private static SLM.LogLevelEnum GetEventSourceLevel(Level level)
        {
            switch (level)
            {
                case Level.Info:
                    return SLM.LogLevelEnum.Informational;
                case Level.Warn:
                    return SLM.LogLevelEnum.Warning;
                case Level.Error:
                    return SLM.LogLevelEnum.Error;
                case Level.Fatal:
                    return SLM.LogLevelEnum.Critical;
                default:
                    return SLM.LogLevelEnum.Verbose;
            }
        }

    }
}
