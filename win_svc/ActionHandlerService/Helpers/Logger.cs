using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.Helpers
{
    public static class Logger
    {
        /// <summary>
        /// Logs text.
        /// </summary>
        /// <param name="text">The text to log.</param>
        /// <param name="message">The ActionMessage that spawned the log.</param>
        /// <param name="tenant">The tenant that spawned the log.</param>
        /// <param name="level">The level to log at.</param>
        public static void Log(string text, ActionMessage message, TenantInformation tenant, LogLevel level)
        {
            var logText = $" {text}. Message: {message.ActionType.ToString()}. Tenant ID: {tenant.TenantId}.";
            switch (level)
            {
                case LogLevel.Debug:
                    BluePrintSys.Messaging.CrossCutting.Logging.Log.Debug(logText);
                    break;
                case LogLevel.Info:
                    BluePrintSys.Messaging.CrossCutting.Logging.Log.Info(logText);
                    break;
                case LogLevel.Warn:
                    BluePrintSys.Messaging.CrossCutting.Logging.Log.Warn(logText);
                    break;
                case LogLevel.Error:
                    BluePrintSys.Messaging.CrossCutting.Logging.Log.Error(logText);
                    break;
                case LogLevel.Fatal:
                    BluePrintSys.Messaging.CrossCutting.Logging.Log.Fatal(logText);
                    break;
            }
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
}
