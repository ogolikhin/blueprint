namespace ImageRenderService.Logging
{
    public interface ILogEntryFilter
    {
        bool Accepts(ILogEntry entry);
    }

    /// <summary>
    /// Filters the log entries according to some condition.
    /// Only log entries that pass the filter acceptance get to be logged.
    /// </summary>
    public interface ILogEntryFilter<in TLogEntry> : ILogEntryFilter where TLogEntry : ILogEntry
    {
        /// <summary>
        /// Determines whether the given log entry passes filter acceptance and gets logged or not.
        /// </summary>
        /// <param name="entry">The log entry to apply filter to.</param>
        /// <returns>True if the log entry passes the filter acceptance, false otherwise.</returns>
        bool Accepts(TLogEntry entry);
    }
}