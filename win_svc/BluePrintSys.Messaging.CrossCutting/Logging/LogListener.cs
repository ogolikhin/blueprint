using System;

namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    public abstract class LogListener<TLogEntry> : ILogListener<TLogEntry> where TLogEntry : ILogEntry
    {
        /// <summary>
        /// LogListener base class.
        /// </summary>
        /// <param name="id">Identifier of the listener.</param>
        /// <param name="name">Name of the listener.</param>
        /// <param name="filter">Filter applied to the incoming log entries.</param>
        /// <param name="formatter">Format used to convert log entry to string.</param>
        protected LogListener(string id, string name, ILogEntryFilter filter, ILogEntryFormatter formatter)
        {
            Id = id;
            Name = name;
            Filter = filter;
            Formatter = formatter;

            IsEnabled = true;
        }

        #region Implementation of ILogListener

        /// <summary>
        /// The identity of the log listener.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The name of the log listener.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Determines which log entries should be logged and which
        /// should not.
        /// </summary>
        public ILogEntryFilter Filter { get; set; }

        /// <summary>
        /// Formats the log entry into a specific format.
        /// </summary>
        public ILogEntryFormatter Formatter { get; set; }

        /// <summary>
        /// Determines whether the log listener is configured correctly and is ready
        /// to process incoming log events.
        /// </summary>
        public virtual bool IsEnabled { get; private set; }

        /// <summary>
        /// Determines whether the log listener accepts the log entry for processing.
        /// </summary>
        /// <param name="entry">The log entry.</param>
        /// <returns>True if the log listener accepts the log entry for processing, otherwise false.</returns>
        public virtual bool Accepts(ILogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (!IsEnabled)
            {
                return false;
            }

            return Filter != null && Filter.Accepts(entry);
        }

        #endregion

        #region Implementation of ILogWriter

        /// <summary>
        /// Writes the log entry into the underlying log.
        /// </summary>
        /// <param name="entry">Entry to write.</param>
        public abstract void Write(TLogEntry entry);

        void ILogWriter.Write(ILogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (typeof(TLogEntry) != entry.GetType())
            {
                throw new ArgumentOutOfRangeException("entry", string.Format("Unexpected Log Entry: {0} ", entry.GetType()));
            }

            Write((TLogEntry) entry);
        }

        #endregion
    }
}