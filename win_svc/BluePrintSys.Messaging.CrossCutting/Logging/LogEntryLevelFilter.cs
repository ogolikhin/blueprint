using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    /// <summary>
    /// ILogEntryFilter implementation for filtering out log entries based on their log level.
    /// </summary>
    public class LogEntryLevelFilter : ILogEntryFilter<StandardLogEntry>
    {
        /// <summary>
        /// All log levels.
        /// </summary>
        private static readonly IEnumerable<Level> AllLevels = new List<Level> { Level.Debug, Level.Info, Level.Warn, Level.Error, Level.Fatal };

        /// <summary>
        /// Creates an instance of LogEntryLevelFilter class.
        /// </summary>
        /// <param name="minimalLogLevel">
        /// Optional parameter indicating desired minimal log level.
        /// If no value is specified, Level.Debug is set by default.
        /// </param>
        public LogEntryLevelFilter(Level minimalLogLevel = Level.Debug)
        {
            AcceptedLevels = AllLevels.Where(level => level >= minimalLogLevel).ToList();
        }

        /// <summary>
        /// Log levels that are permitted to pass through the filter.
        /// </summary>
        public IEnumerable<Level> AcceptedLevels { get; private set; }

        #region Implementation of ILogEntryFilter

        /// <summary>
        /// Determines whether the given log entry passes filter acceptance and gets logged or not.
        /// </summary>
        /// <param name="entry">The log entry to apply filter to.</param>
        /// <returns>True if the log entry passes the filter acceptance, false otherwise.</returns>
        public bool Accepts(StandardLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            return AcceptedLevels.Contains(entry.Level);
        }

        bool ILogEntryFilter.Accepts(ILogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            var standardLogEntry = entry as StandardLogEntry;

            return standardLogEntry != null && Accepts(standardLogEntry);
        }

        #endregion Implementation of ILogEntryFilter
    }
}