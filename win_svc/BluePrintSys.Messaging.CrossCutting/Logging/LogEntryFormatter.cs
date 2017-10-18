using System;
using System.Globalization;

namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    /// <summary>
    ///
    /// </summary>
    public abstract class LogEntryFormatter<TLogEntry> : ILogEntryFormatter<TLogEntry> where TLogEntry : ILogEntry
    {
        protected string LogId;

        protected LogEntryFormatter(string logId)
        {
            LogId = logId;
        }

        #region Implementation of ILogEntryFormatter

        /// <summary>
        /// Formats the log entries.
        /// </summary>
        /// <param name="entry">The log entry to be formatted.</param>
        /// <returns>
        /// The result of the formatting.
        /// </returns>
        public abstract object Format(TLogEntry entry);

        object ILogEntryFormatter.Format(ILogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (typeof(TLogEntry) != entry.GetType())
            {
                throw new ArgumentOutOfRangeException("entry", string.Format(CultureInfo.InvariantCulture, "Unexpected Log Entry: {0}", entry.GetType()));
            }

            return Format((TLogEntry)entry);
        }

        #endregion

        /// <summary>
        /// Local time zone UTC offset
        /// </summary>
        /// <returns></returns>
        protected string TimeZoneOffset()
        {
            if (TimeZoneInfo.Local.BaseUtcOffset.Ticks == 0)
            {
                return "GMT";
            }

            return String.Format
            (
                CultureInfo.InvariantCulture,
                "GMT{0}{1}",
                TimeZoneInfo.Local.BaseUtcOffset.Ticks > 0 ? "+" : "-",
                TimeZoneInfo.Local.BaseUtcOffset.ToString("hh\\:mm", CultureInfo.InvariantCulture)
            );
        }

        /// <summary>
        /// Escape CSV text.
        ///     - Double quote quotes
        ///     - Wrap the whole text in quotes if contains line breaks or quotes
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected string CsvEscapeText(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                if (text.IndexOfAny("\",\x0A\x0D".ToCharArray()) > -1)
                {
                    text = String.Format(CultureInfo.InvariantCulture, "\"{0}\"", text.Replace("\"", "\"\""));
                }
            }

            return text;
        }
    }
}