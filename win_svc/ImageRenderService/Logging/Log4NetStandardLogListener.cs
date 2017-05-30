using System;
using System.Linq;
using System.Configuration;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using ImageRenderService.Helpers;


namespace ImageRenderService.Logging
{

    /// <summary>
    /// Provides ad hoc logging functionality.
    /// </summary>
    /// <remarks>This class is the base functionality that is also shared by LogWriter in the data access project</remarks>
    public class Log4NetStandardLogListener : LogListener<StandardLogEntry>, IFileLogListener
    {
        /// <summary>
        /// Locking object.
        /// </summary>
        protected static readonly object Padlock = new object();
        
        /// <summary>
        /// Interface used for all logging to Log4Net
        /// </summary>
        protected ILog Log;

        /// <summary>
        /// This is a singlton which creates a class the first time it it referenced.  We
        /// need to use this version because we need to be able to clear the variables
        /// as well as ensure we can re-create it if it is cleared
        /// See http://www.yoda.arachsys.com/csharp/singleton.html
        /// </summary>
        private static ILogListener _instance;

        /// <summary>
        /// Returns an initialized logger.
        /// </summary>
        /// <remarks>
        /// The reason it is public is that we want any of the higher level projects to use LogWriter.Instance for errors
        /// </remarks>
        public static ILogListener Instance
        {
            get
            {
                lock (Padlock)
                {
                    if (_instance == null)
                    {
                        // Create a new instance of Logger
                        _instance = new Log4NetStandardLogListener();
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Returns whether the logger is enabled
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                //HACK: This works because Emergency is higher than anything we have defined, but is still turned off when it's disabled
                return Log != null && Log.Logger.IsEnabledFor(log4net.Core.Level.Emergency);
            }
        }

        /// <summary>
        /// Returns whether debug messages are currently set
        /// </summary>
        public bool IsDebugEnabled
        {
            get
            {
                return Log != null && Log.IsDebugEnabled;
            }
        }

        /// <summary>
        /// Returns whether error messages are currently set
        /// </summary>
        public bool IsErrorEnabled
        {
            get
            {
                return Log != null && Log.IsErrorEnabled;
            }
        }

        /// <summary>
        /// Returns whether warn messages are currently set
        /// </summary>
        public bool IsWarnEnabled
        {
            get
            {
                return Log != null && Log.IsWarnEnabled;
            }
        }

        /// <summary>
        /// Returns whether fatal messages are currently set
        /// </summary>
        public bool IsFatalEnabled
        {
            get
            {
                return Log != null && Log.IsFatalEnabled;
            }
        }

        /// <summary>
        /// Returns whether info messages are currently set
        /// </summary>
        public bool IsInfoEnabled
        {
            get
            {
                return Log != null && Log.IsInfoEnabled;
            }
        }

        /// <summary>
        /// Perform all initialization for the logger, including layout pattern and file location.
        /// </summary>
        protected Log4NetStandardLogListener()
            : base
        (
            "Server",
            "ImageGenServiceLogger",
            new LogEntryLevelFilter(),
            new Log4NetStandardLogEntryFormatter("Server")
        )
        {
            // Get the logger hierarchy so we can try to find errors later
            var loggerHierarchy = GetCurrentHierarchy();


            // Check the existence of appenders
            if (!loggerHierarchy.GetAppenders().Any())
            {
                var message = string.Format("No appenders were added.  Appenders and a root element need to be defined in the config file.");
                throw new Exception(message);
            }

            //HACK: There is very little that can be done with this to make sure it was loaded properly.  There seems to be no way to make sure the root was properly setup.
            /* Tried to:
             * -Create a logger and add the appender
             * -Look for any property on the ILogWriter to tell if it was a non-default logger
             */
            Log = log4net.LogManager.GetLogger(Name);
        }

        /// <summary>
        /// Destructor, which clears all variables if they have not already been cleared
        /// </summary>
        ~Log4NetStandardLogListener()
        {
            ClearLoggers();
        }

        /// <summary>
        /// This will clear the existing logger
        /// </summary>
        /// <returns>Returns true if there was one to clear</returns>
        public static bool Clear()
        {
            //We need to lock this because we might have threads coming in at the same as as the thread is being cleared
            lock (Padlock)
            {
                // Check if we need to clear it or not
                if (_instance != null)
                {
                    LogManager.Manager.RemoveListener(_instance);

                    var logWriter = _instance as Log4NetStandardLogListener;
                    if (logWriter != null)
                    {
                        logWriter.ClearLoggers();
                    }

                    _instance = null;

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Disposes of all of the loggers we created, but do not reference
        /// </summary>
        private void ClearLoggers()
        {
            if (Log == null)
            {
                return;
            }

            // Clear up any of the loggers we created
            var loggerHierarchy = (Hierarchy)Log.Logger.Repository;
            loggerHierarchy.Clear();

            Log = null;
            _file = null;
        }

        /// <summary>
        /// This will get the current Log4Net hierarchy, which has the loggers and appenders
        /// </summary>
        /// <returns></returns>
        private Hierarchy GetCurrentHierarchy()
        {
            return (Hierarchy)log4net.LogManager.GetRepository();
        }
        
        #region Implementation of ILogWriter

        /// <summary>
        /// Writes the log entry into the underlying log.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        public override void Write(StandardLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (Filter != null && !Filter.Accepts(entry))
            {
                return;
            }

            var message = Formatter.Format(entry);

            switch (entry.Level)
            {
                case Level.Info:
                    Log.Info(message);
                    //if (IsInfoEnabled) LoggingService.LogToLoggingService(entry, message);
                    break;

                case Level.Warn:
                    Log.Warn(message);
                    //if (IsWarnEnabled) LoggingService.LogToLoggingService(entry, message);
                    break;

                case Level.Error:
                    Log.Error(message);
                    //if (IsErrorEnabled) LoggingService.LogToLoggingService(entry, message);
                    break;

                case Level.Fatal:
                    Log.Fatal(message);
                    //if (IsFatalEnabled) LoggingService.LogToLoggingService(entry, message);
                    break;

                default:
                    Log.Debug(message);
                    //if (IsDebugEnabled) LoggingService.LogToLoggingService(entry, message);
                    break;
            }

        }

        #endregion

        #region Implementation of IFileLogListener

        private string _file;

        /// <summary>
        /// Represents the file path to the written file.
        /// This method is used in ContentDomainService to download server logs.
        /// </summary>
        public string File
        {
            get
            {
                if (string.IsNullOrEmpty(_file))
                {
                    var hierarchy = GetCurrentHierarchy();

                    if (hierarchy == null || hierarchy.Root == null)
                    {
                        Log.Warn("log4net hierarchy or hierarchy root is null.");
                        _file = string.Empty;
                    }
                    else
                    {
                        //TODO: Somehow make this work for multiple file appenders. Currently only considering the first file appender.
                        var fileAppender = hierarchy.GetAppenders().OfType<FileAppender>().FirstOrDefault();
                        _file = fileAppender == null ? string.Empty : fileAppender.File;
                    }
                }

                return _file;
            }
        }

        #endregion
    }
}