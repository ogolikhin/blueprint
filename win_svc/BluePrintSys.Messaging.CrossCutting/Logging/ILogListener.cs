namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    public interface ILogListener : ILogWriter
    {
        /// <summary>
        /// The identity of the log listener.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The name of the log listener.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Determines which log entries should be logged and which
        /// should not.
        /// </summary>
        ILogEntryFilter Filter { get; set; }

        /// <summary>
        /// Formats the log entry into a specific format.
        /// </summary>
        ILogEntryFormatter Formatter { get; set; }

        /// <summary>
        /// Determines whether the log listener is configured correctly and is ready
        /// to process incoming log events.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Determines whether the log listener accepts the log entry for processing.
        /// </summary>
        /// <param name="entry">The log entry.</param>
        /// <returns>True if the log listener accepts the log entry for processing, otherwise false.</returns>
        bool Accepts(ILogEntry entry);
    }

    /// <summary>
    /// Represents a specific logging mechanism that listens for events
    /// from the Log Manager and writes them to the underlying log.
    /// </summary>
    public interface ILogListener<TLogEntry> : ILogListener, ILogWriter<TLogEntry> where TLogEntry : ILogEntry
    {
    }
}