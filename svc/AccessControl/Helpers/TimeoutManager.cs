using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using ServiceLibrary.Repositories.ConfigControl;

namespace AccessControl.Helpers
{
    internal class TimeoutManager<T> : ITimeoutManager<T>
        where T : IComparable<T>
    {
        private readonly object _lock = new object();
        private readonly IServiceLogRepository _log;
        private DateTime? _nextTimeout;

        internal readonly IDictionary<Key, Action> Items = new SortedDictionary<Key, Action>();
        internal readonly IDictionary<T, DateTime> TimeoutsByItem = new Dictionary<T, DateTime>();
        internal ITimer Timer;

        public TimeoutManager()
            : this(new TimerWrapper(), new ServiceLogRepository())
        {
        }

        internal TimeoutManager(ITimer timer, IServiceLogRepository log)
        {
            Timer = timer;
            _log = log;
            timer.AutoReset = false;
            Timer.Elapsed += TimerOnElapsed;
        }

        public void Insert(T item, DateTime timeout, Action callback)
        {
            lock (_lock)
            {
                DateTime oldTimeout;
                if (TimeoutsByItem.TryGetValue(item, out oldTimeout))
                {
                    Items.Remove(new Key(oldTimeout, item));
                }
                TimeoutsByItem.Remove(item);
                Items.Add(new Key(timeout, item), () =>
                {
                    Remove(item);
                    callback();
                });
                TimeoutsByItem.Add(item, timeout);
                UpdateTimer();
            }
        }

        public void Remove(T item)
        {
            lock (_lock)
            {
                DateTime timeout;
                if (TimeoutsByItem.TryGetValue(item, out timeout))
                {
                    Items.Remove(new Key(timeout, item));
                    TimeoutsByItem.Remove(item);
                    UpdateTimer();
                }
            }
        }

        private void UpdateTimer(bool force = false)
        {
            var nextTimeout = Items.Any() ? Items.First().Key.Timeout : (DateTime?)null;
            if (force || _nextTimeout != nextTimeout)
            {
                _nextTimeout = nextTimeout;

                if (Timer == null)
                {
                    throw new ObjectDisposedException("TimeoutManager has been disposed.");
                }
                Timer.Enabled = false;
                if (_nextTimeout.HasValue)
                {
                    var interval = (_nextTimeout.Value - Timer.Now()).TotalMilliseconds + TimerWrapper.ExtraInterval;
                    if (interval <= 0)
                    {
                        TimeoutItems();
                    }
                    else
                    {
                        Timer.Interval = interval;
                        Timer.Enabled = true;
                    }
                }
                else
                {
                    Timer.Enabled = false;
                }
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                TimeoutItems();
            }
            catch (Exception ex)
            {
                _log.LogError(WebApiConfig.LogSourceSessions, ex);
            }
        }

        private void TimeoutItems()
        {
            lock (_lock)
            {
                var timeouts = Items.TakeWhile(e => e.Key.Timeout <= Timer.Now()).ToList();
                foreach (var entry in timeouts)
                {
                    Items.Remove(entry.Key);
                    TimeoutsByItem.Remove(entry.Key.Item);
                    entry.Value();
                }
                UpdateTimer(true);
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Timer.Elapsed -= TimerOnElapsed;
                Timer.Dispose();
                Timer = null;
            }
        }

        #endregion IDisposable

        internal struct Key : IComparable<Key>
        {
            public DateTime Timeout { get; }
            public T Item { get; }

            public Key(DateTime timeout, T item)
            {
                Timeout = timeout;
                Item = item;
            }

            public int CompareTo(Key other)
            {
                int result = Timeout.CompareTo(other.Timeout);
                if (result == 0)
                {
                    result = Item.CompareTo(other.Item);
                }
                return result;
            }
        }
    }

    /// <summary>
    /// A wrapper for <see cref="System.Timers.Timer"/> that implements <see cref="ITimer"/>.
    /// </summary>
    internal class TimerWrapper : ITimer
    {
        internal const double ExtraInterval = 20.0;
        private readonly Timer _timer = new Timer();

        public DateTime Now() { return DateTime.UtcNow; }

        public bool AutoReset
        {
            get { return _timer.AutoReset; }
            set { _timer.AutoReset = value; }
        }

        public bool Enabled
        {
            get { return _timer.Enabled; }
            set { _timer.Enabled = value; }
        }

        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public event ElapsedEventHandler Elapsed
        {
            add { _timer.Elapsed += value; }
            remove { _timer.Elapsed -= value; }
        }

        public void Dispose() { _timer.Dispose(); }
    }
}
