using System;
using System.Collections.Generic;
using System.IO;

namespace Common
{
    public class LogFile : IDisposable
    {
        #region Member variables

        private static Dictionary<string, object> _locks = new Dictionary<string, object>();

        private string _filename = null;
        private bool _isDisposed = false;
        private StreamWriter _writer = null;

        #endregion Member variables

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename">The filename for the log file.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]    // StreamWriter.Dispose() already does this for us.
        public LogFile(string filename)
        {
            _filename = filename;

            lock (_locks)
            {
                if (!_locks.ContainsKey(_filename))
                {
                    _locks.Add(_filename, this); // We need to lock writes across all instances of the same file.
                }
            }

            _writer = new StreamWriter(File.Open(_filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~LogFile()
        {
            Dispose(false);
        }

        #region IDisposable members

        /// <summary>
        /// Disposes this object explicitly.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the members of this object.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called in the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_writer != null)
                    {
                        Logger.WriteDebug("Disposing the StreamWriter...");
                        _writer.Dispose();
                    }
                }

                _writer = null;

                _isDisposed = true;
            }
        }

        #endregion IDisposable members

        /// <summary>
        /// Writes a string to the log file followed by a new line.
        /// </summary>
        /// <param name="line">The string to write.</param>
        public void WriteLine(string line)
        {
            WriteLine(line, null);
        }

        /// <summary>
        /// Writes a formatted string to the log file followed by a new line.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="args">The arguments for the formatted string.</param>
        public void WriteLine(string format, params Object[] args)
        {
            lock (_locks[_filename])
            {
                string datetime = DateTime.Now.ToStringInvariant("u");   // The 'u' formats like this:  2008-06-15 21:15:07Z
                format = I18NHelper.FormatInvariant("{0}: {1}", datetime, format);

                if (args != null)
                {
                    _writer.WriteLine(format, args);
                }
                else
                {
                    _writer.WriteLine(format);
                }

                _writer.Flush();
            }
        }
    }
}
