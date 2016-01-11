using System;
using System.IO;
using System.Runtime.Serialization;

namespace Common
{
    /// <summary>
    /// Thread-safe logging function
    /// 
    /// This class contains methods that print statements into logs. Currently, statements are printed to standard output or standard error.   
    /// </summary>
    public static class Logger
    {
        #region Private Member Variables

        private static int _logLevel = (int)LogLevels.INFO;
        private static readonly object _lock = new object();

        #endregion

        #region Private Methods

        /// <summary>
        /// Overloaded thread-safe method. Given the log level, this prints a string to a text stream
        /// </summary>
        /// <param name="level">The type of message denoted by the log level .</param>
        /// <param name="console">The stream to where the output is redirected.</param>
        /// <param name="msg">The string to be printed.</param>
        private static void write(LogLevels level, TextWriter console, string msg)
        {
            write(level, console, msg, null);
        }

        /// <summary>
        /// Overloaded thread-safe method. Given the log level, this formats a string and sends it to a text stream
        /// </summary>
        /// <param name="level">The type of message denoted by the log level .</param>
        /// <param name="console">The stream to where the output is redirected.</param>
        /// <param name="format">A format string.</param>
        /// <param name="msg">The string to be printed.</param>
        private static void write(LogLevels level, TextWriter console, string format, params Object[] msg)
        {
            if (((int)LogLevel & (int)level) == (int)level)
            {
                string datetime = DateTime.Now.ToStringInvariant("u");   // The 'u' formats like this:  2008-06-15 21:15:07Z
                format = I18NHelper.FormatInvariant("{0} {1}: {2}", datetime, level.ToString(), format);

                lock (_lock)
                {
                    if (msg == null) { console.WriteLine(format); }
                    else { console.WriteLine(format, msg); }
                }
            }
        }

        #endregion

        #region Public Enums

        /// <summary>
        /// Specific values for the logging levels ( Error, Warning, Info, Debug, Trace )
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags"), Flags]   // Ignore this warning.
        [DataContract]
        public enum LogLevels
        {
            [EnumMember]
            NONE = 0,
            [EnumMember]
            ERROR = 1,
            [EnumMember]
            WARNING = 3,  // Includes ERROR + WARNING
            [EnumMember]
            INFO = 7,     // Includes ERROR + WARNING + INFO
            [EnumMember]
            DEBUG = 15,   // Includes ERROR + WARNING + INFO + DEBUG
            [EnumMember]
            TRACE = 31    // Includes ERROR + WARNING + INFO + DEBUG + TRACE
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// This property activates the different log levels.
        /// </summary>
        public static LogLevels LogLevel
        {
            get { return (LogLevels)_logLevel; }
            set {
                Console.WriteLine("*** Setting LogLevel to: '{0}'", value.ToString());
                _logLevel = (int)value; }
        }

        #endregion

        #region Public Methods

        // The following are functions for each of the different log levels

        /// <summary>
        /// Overloaded thread-safe method. This prints a standard message as a string to standard output.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public static void WriteInfo(string msg)
        {
            write(LogLevels.INFO, Console.Out, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints a standard message as an array of string to standard output in a
        /// specific format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="msg">The array of strings to be printed.</param>
        public static void WriteInfo(string format, params Object[] msg)
        {
            write(LogLevels.INFO, Console.Out, format, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints a trace message as a string to standard output.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public static void WriteTrace(string msg)
        {
            write(LogLevels.TRACE, Console.Out, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints a trace message as an array of string to standard output in a
        /// specific format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="msg">The array of strings to be printed.</param>
        public static void WriteTrace(string format, params Object[] msg)
        {
            write(LogLevels.TRACE, Console.Out, format, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints Debug message as a string to standard output.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public static void WriteDebug(string msg)
        {
            write(LogLevels.DEBUG, Console.Out, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints a debug message as an array of string to standard output in a
        /// specific format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="msg">The array of strings to be printed.</param>
        public static void WriteDebug(string format, params Object[] msg)
        {
            write(LogLevels.DEBUG, Console.Out, format, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints a warning message as a string to standard error.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public static void WriteWarning(string msg)
        {
            write(LogLevels.WARNING, Console.Error, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints a warning message as an array of string to standard error in a
        /// specific format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="msg">The array of strings to be printed.</param>
        public static void WriteWarning(string format, params Object[] msg)
        {
            write(LogLevels.WARNING, Console.Error, format, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints an eror message as a string to standard error.
        /// </summary>
        /// <param name="msg">The string to be printed.</param>
        public static void WriteError(string msg)
        {
            write(LogLevels.ERROR, Console.Error, msg);
        }

        /// <summary>
        /// Overloaded thread-safe method. This prints an error message as an array of string to standard error in a
        /// specific format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="msg">The array of strings to be printed.</param>
        public static void WriteError(string format, params Object[] msg)
        {
            write(LogLevels.ERROR, Console.Error, format, msg);
        }

        /// <summary>
        /// Given the log level, this determines whether logging is enabled 
        /// </summary>
        public static bool IsEnabled(LogLevels mode)
        {
            return (((int)LogLevel & (int)mode) == (int)mode);
        }

        #endregion
    }
}
