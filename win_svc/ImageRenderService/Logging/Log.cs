using System;
using ImageRenderService.Annotations;

namespace ImageRenderService.Logging
{
    /// <summary>
    /// Logging utility class. Entry point for logging information.
    /// </summary>
    public static class Log
    {
        #region Logging

        /// <summary>
        /// Logs a message at the Debug level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void Debug(object message)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Debug,
                Entry = message,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a formatted message at the Debug level.
        /// </summary>
        /// <param name="format">Formatted message to log.</param>
        /// <param name="args">Variable number of parameters to be used with the formatted string.</param>
        public static void DebugFormat(string format, params object[] args)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Debug,
                Entry = string.Format(format, args),
                Format = format,
                Arguments = args,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a message at the Info level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void Info(object message)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Info,
                Entry = message,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a formatted message at the Info level.
        /// </summary>
        /// <param name="format">Formatted message to log.</param>
        /// <param name="args">Variable number of parameters to be used with the formatted string.</param>
        public static void InfoFormat(string format, params object[] args)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Info,
                Format = format,
                Arguments = args,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a message at the Warning level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void Warn(object message)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Warn,
                Entry = message,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a message with exception at the Warning level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="exception">Exception details.</param>
        public static void Warn(object message, Exception exception)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Warn,
                Entry = message,
                Exception = exception,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a formatted message at the Warning level.
        /// </summary>
        /// <param name="format">Formatted message to log.</param>
        /// <param name="args">Variable number of parameters to be used with the formatted string.</param>
        public static void WarnFormat(string format, params object[] args)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Warn,
                Format = format,
                Arguments = args,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a message at the Error level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void Error(object message)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Error,
                Entry = message,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a message with exception at the Error level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="exception">Exception details.</param>
        public static void Error(object message, Exception exception)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Error,
                Entry = message,
                Exception = exception,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a formatted message at the Error level.
        /// </summary>
        /// <param name="format">Formatted message to log.</param>
        /// <param name="args">Variable number of parameters to be used with the formatted string.</param>
        public static void ErrorFormat(string format, params object[] args)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Error,
                Format = format,
                Arguments = args,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a message at the Fatal level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void Fatal(object message)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Fatal,
                Entry = message,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a message with exception at the Fatal level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="exception">Exception details.</param>
        public static void Fatal(object message, Exception exception)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Fatal,
                Entry = message,
                Exception = exception,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        /// <summary>
        /// Logs a formatted message at the Fatal level.
        /// </summary>
        /// <param name="format">Formatted message to log.</param>
        /// <param name="args">Variable number of parameters to be used with the formatted string.</param>
        public static void FatalFormat(string format, params object[] args)
        {
            var logEntry = new StandardLogEntry
            {
                Level = Level.Fatal,
                Format = format,
                Arguments = args,
                DateTime = DateTime.Now,
                UserName = GetCurrentUserName(),
                SessionId = GetCurrentSessionId()
            };

            LogManager.Log.Write(logEntry);
        }

        #endregion

        #region Asserting

        /// <summary>
        /// Assert a condition.
        /// In Debug mode delegates the call to Log.Assert, in Release mode logs the assertion as a debug entry.
        /// </summary>
        /// <param name="condition">Condition.</param>
#if DEBUG
        [ContractAnnotation("condition:false => halt")]
#endif
        public static void Assert(bool condition)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(condition);
#else
            if (!condition)
            {
                WarnFormat("Assertion: Call stack: {0}.", GetStackTrace(2));
            }
#endif
        }

        /// <summary>
        /// Asserts a condition.
        /// In Debug mode delegates the call to Log.Assert, in Release mode logs the assertion as a debug entry.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="message">The message to display when the assertion of the condition fails.</param>
#if DEBUG
        [ContractAnnotation("condition:false => halt")]
#endif
        public static void Assert(bool condition, string message)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(condition, message);
#else
            if (!condition)
            {
                WarnFormat("Assertion: Message {0}; Call stack: {1}.", message, GetStackTrace(2));
            }
#endif
        }

        /// <summary>
        /// Asserts a condition.
        /// In Debug mode delegates the call to Log.Assert, in Release mode logs the assertion as a debug entry.
        /// </summary>
        /// <param name="condition">Condition to assert.</param>
        /// <param name="message">Message to display when the assertion of the condition fails.</param>
        /// <param name="detailMessage">Detailed message supplementing the primary message.</param>
#if DEBUG
        [ContractAnnotation("condition:false => halt")]
#endif
        public static void Assert(bool condition, string message, string detailMessage)
        {
#if DEBUG
#pragma warning disable 618
            System.Diagnostics.Debug.Assert(condition, message, detailMessage);
#pragma warning restore 618
#else
            if (!condition)
            {
                WarnFormat("Assertion: Message {0}; Detailed Message {1}; Call stack: {2}.", message, detailMessage, GetStackTrace(2));
            }
#endif
        }

        /// <summary>
        /// Assert a condition.
        /// In Debug mode delagest the call to Log.Assert, in Release mode logs the assertion as a debug entry.
        /// </summary>
        /// <param name="condition">Condition to assert.</param>
        /// <param name="message">Message to display when the assertion of the condition fails.</param>
        /// <param name="detailMessageFormat">Detailed message supplementing the primary message as a formatted string.</param>
        /// <param name="args">Variable number of parameters to be used with the formatted string.</param>
#if DEBUG
        [ContractAnnotation("condition:false => halt")]
#endif
        public static void Assert(bool condition, string message, string detailMessageFormat, params object[] args)
        {
            Assert(condition, message, string.Format(detailMessageFormat, args));
        }

#if !DEBUG
        /// <summary>
        /// Get trace stack.
        /// </summary>
        /// <param name="skipFrames">Number of stack frames to skip.</param>
        /// <returns>String representation of the stack trace.</returns>
        private static string GetStackTrace(int skipFrames)
        {
            try
            {
                IStackTraceProvider stackTraceProvider = StackTraceProvider.Current;
                if (stackTraceProvider != null)
                {
                    return StackTraceProvider.Current.GetStackTrace(skipFrames);
                }
            }
            catch { }
            return string.Empty;
        }
#endif

        #endregion

        #region Context

        //TODO: Need to find a better way to inject the context at runtime

        // Current user
        public static Func<string> GetCurrentUserName = () => null;

        // Current session id
        public static Func<string> GetCurrentSessionId = () => null;

        #endregion
    }
}
