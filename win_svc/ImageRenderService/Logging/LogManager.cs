using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageRenderService.Collections;

namespace ImageRenderService.Logging
{
    /// <summary>
    /// Log manager singleton
    /// Asynchronously delivers log entries to a collection of listeners
    /// </summary>
    public sealed partial class LogManager : ILogManager, ILogWriter<StandardLogEntry>
    {
        /// <summary>
        /// The cancellation token source
        /// </summary>
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        /// <summary>
        /// BackgroundWorker is used for asynchronously delivering log entries to listeners
        /// Log entries delivery filtering is based on listeneres log levels
        /// When the app domain will unload, the thread will be aborted, before the LogManager will be garbage collected (if a finalizer would be implemented, the finalizer would be called before the it is garbage collected)
        /// </summary>
        private readonly Task _backgroundTask;

        /// <summary>
        /// The singleton instance
        /// </summary>
        private static volatile LogManager _instance;
        private static readonly object syncRoot = new Object();

        /// <summary>
        /// Log listeneres
        /// </summary>
        private readonly List<ILogListener> _logListeners;

        /// <summary>
        /// Queued log entries. To be delivered asynchronously to the listeners
        /// </summary>
        private readonly IQueue<ILogEntry> _logEntries;

        #region Construction

        /// <summary>
        /// Constructor
        /// </summary>
        private LogManager()
        {
            try
            {
                _logListeners = new List<ILogListener>();
                _logEntries = new ConcurrentPriorityQueue<ILogEntry>();

                _backgroundTask = new Task(() => BackgroundThread(this, _tokenSource.Token), _tokenSource.Token);
                _backgroundTask.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to initialize the log manager. Excpetion {0}", e);
            }
        }

        #endregion

        #region Instance access

        /// <summary>
        /// Log manager instance (ILogManager)
        /// </summary>
        public static ILogManager Manager
        {
            get
            {
                return GetLogManagerInstance();
            }
        }

        /// <summary>
        /// Log manager instance (ILogWriter)
        /// </summary>
        public static ILogWriter<StandardLogEntry> Log
        {
            get
            {
                return GetLogManagerInstance();
            }
        }
        
        private static LogManager GetLogManagerInstance()
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new LogManager();
                    }
                }
            }
            return _instance;
        }

        public static void Write(ILogEntry entry)
        {
            GetLogManagerInstance().EnqueLogEntry(entry);
        }

        #endregion

        #region Implementation of ILogManager

        /// <summary>
        /// Add listener
        /// </summary>
        /// <param name="logListener"></param>
        void ILogManager.AddListener(ILogListener logListener)
        {
            lock (_logListeners)
            {
                _logListeners.Add(logListener);
            }
        }

        /// <summary>
        /// Remove listener
        /// </summary>
        /// <param name="logListener"></param>
        public void RemoveListener(ILogListener logListener)
        {
            lock (_logListeners)
            {
                _logListeners.Remove(logListener);
            }
        }

        /// <summary>
        /// Listeners
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ILogListener> GetListeners()
        {
            //return a copy of the internal list
            List<ILogListener> listeners;

            lock (_logListeners)
            {
                listeners = new List<ILogListener>(_logListeners);
            }

            return listeners;
        }

        /// <summary>
        /// removes all listeners
        /// </summary>
        public void ClearListeners()
        {
            lock (_logListeners)
            {
                _logListeners.Clear();
            }
        }

        /// <summary>
        /// Call this if you want to explicity shut down the log manager when the process is still waiting to log
        /// </summary>
        public void CloseManager()
        {
            ClearListeners();

            // We have to kill the thread since it's sleeping waiting for log to be written
            if (_backgroundTask != null)
            {
                _tokenSource.Cancel();
                Logging.Log.Debug("LogManager.CloseManager has been called");
            }
        }

        #endregion Implementation of ILogManager

        #region Implementation of ILogWriter<StandardLogEntry>

        /// <summary>
        /// Write implementation
        /// Entries are queued internally and asynchronously delivered to listeners
        /// </summary>
        /// <param name="entry"></param>
        public void Write(StandardLogEntry entry)
        {
            EnqueLogEntry(entry);
        }

        #endregion Implementation of ILogWriter<StandardLogEntry>

        //#region Implementation of ILogWriter<PerformanceLogEntry>

        //public void Write(PerformanceLogEntry entry)
        //{
        //    EnqueLogEntry(entry);
        //}

        //#endregion Implementation of ILogWriter<PerformanceLogEntry>

        #region Implementation of asyncronous Write

        /// <summary>
        /// Implementation of the background message processing.
        /// </summary>
        /// <param name="logManager">The LogManager.</param>
        /// <param name="token"></param>
        private static void BackgroundThread(LogManager logManager, CancellationToken token)
        {
            do
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    //deliver one log entry to log listeners
                    var logEntry = logManager.DequeueLogEntry();

                    // deliver it
                    logManager.DeliverLogEntry(logEntry);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to process the queued entry on the background thread. Exception {0}", e);
                }
            }
            while (true);
        }

        private ILogEntry DequeueLogEntry()
        {
            return _logEntries.Dequeue();
        }

        /// <summary>
        /// Deliver log entry to log listeners
        /// </summary>
        /// <param name="entry"></param>
        private void DeliverLogEntry(ILogEntry entry)
        {
            lock (_logListeners)
            {
                foreach (var logListener in _logListeners.Where(l => l != null && l.Accepts(entry)))
                {
                    try
                    {
                        logListener.Write(entry);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Failed to write log entry to log listener '{0}'. Exception occurred: {1}", logListener.Name ?? "unknown", e);
                    }
                }
            }
        }

        #endregion

        private void EnqueLogEntry(ILogEntry entry)
        {
            try
            {
                _logEntries.Enqueue(entry);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to enque the entry. Excpetion {0}", e);
            }
        }
    }
}
