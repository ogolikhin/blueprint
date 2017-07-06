using System;
using log4net;

namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    /// <summary>
    /// Formats the message for log4net to consume
    /// </summary>
    public class Log4NetStandardLogEntryFormatter : LogEntryFormatter<StandardLogEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetStandardLogEntryFormatter" /> class.
        /// </summary>
        /// <param name="logId">The log id.</param>
        public Log4NetStandardLogEntryFormatter(string logId) : base(logId) { }

        #region Overrides of LogEntryFormatter

        /// <summary>
        /// Formats the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">entry</exception>
        public override object Format(StandardLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            LogicalThreadContext.Properties["logId"] = LogId;
            LogicalThreadContext.Properties["timeZoneOffset"] = TimeZoneOffset();
            LogicalThreadContext.Properties["sessionId"] = string.IsNullOrWhiteSpace(entry.SessionId) ? string.Empty : entry.SessionId;
            LogicalThreadContext.Properties["userName"] = string.IsNullOrWhiteSpace(entry.UserName) ? string.Empty : entry.UserName;
            LogicalThreadContext.Properties["level"] = entry.Level.ToString();

            var formattedLogEntry = CsvEscapeText(entry.GetContent());

            return formattedLogEntry;
        }

        #endregion
    }
}