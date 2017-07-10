namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    public interface ILogWriter
    {
        void Write(ILogEntry entry);
    }

    /// <summary>
    /// Logs the log entries into the actual log.
    /// </summary>
    public interface ILogWriter<in TLogEntry> where TLogEntry : ILogEntry
    {
        /// <summary>
        /// Writes the log entry into the underlying log.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        void Write(TLogEntry entry);
    }
}