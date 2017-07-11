using System;
using BluePrintSys.Messaging.CrossCutting.Collections;

namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    /// <summary>
    /// Represents an entry in the log.
    /// </summary>
    public abstract class LogEntry : ILogEntry
    {
        /// <summary>
        /// The date and time of the log entry.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The session context of the log entry.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// The user context of the log entry.
        /// </summary>
        public string UserName { get; set; }

        public abstract string GetContent();
        public virtual Priority GetPriority()
        {
            return Priority.LOW;
        }
    }
}