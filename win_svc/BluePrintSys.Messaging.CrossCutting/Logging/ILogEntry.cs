using System;
using BluePrintSys.Messaging.CrossCutting.Collections;

namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    public interface ILogEntry : IHavePriority
    {
        /// <summary>
        /// The date and time of the log entry.
        /// </summary>
        DateTime DateTime { get; set; }

        /// <summary>
        /// The session context of the log entry.
        /// </summary>
        string SessionId { get; set; }

        /// <summary>
        /// The user context of the log entry.
        /// </summary>
        string UserName { get; set; }

        string GetContent();
    }
}