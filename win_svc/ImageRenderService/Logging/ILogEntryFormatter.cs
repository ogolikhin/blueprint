namespace ImageRenderService.Logging
{
    public interface ILogEntryFormatter
    {
        object Format(ILogEntry entry);
    }

    /// <summary>
    /// Formats the log entries.
    /// </summary>
    public interface ILogEntryFormatter<in TLogEntry> : ILogEntryFormatter where TLogEntry : ILogEntry
    {
        /// <summary>
        /// Formats the log entries.
        /// </summary>
        /// <param name="entry">The log entry to be formatted.</param>
        /// <returns>The result of the formatting.</returns>
        object Format(TLogEntry entry);
    }
}