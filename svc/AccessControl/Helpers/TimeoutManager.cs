using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace AccessControl.Helpers
{
    internal class TimeoutManager<T> : ITimeoutManager<T>
    {
        private readonly object _lock = new object();
        internal readonly IDictionary<DateTime, Tuple<T, Action>> Items = new SortedDictionary<DateTime, Tuple<T, Action>>();
        internal readonly IDictionary<T, DateTime> TimeoutsByItem = new Dictionary<T, DateTime>();
        internal ITimer Timer;
        private DateTime _nextTimeout;

        public TimeoutManager()
            : this(new TimerWrapper())
        {
        }

        internal TimeoutManager(ITimer timer)
        {
            Timer = timer;
            Timer.Elapsed += TimerOnElapsed;
        }

        public void Insert(T item, DateTime timeout, Action callback)
        {
            lock (_lock)
            {
                DateTime oldTimeout;
                if (TimeoutsByItem.TryGetValue(item, out oldTimeout))
                {
                    Items.Remove(oldTimeout);
                }
                TimeoutsByItem.Remove(item);
                Items.Add(timeout, new Tuple<T, Action>(item, () =>
                {
                    Remove(item);
                    callback();
                }));
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
                    Items.Remove(timeout);
                    TimeoutsByItem.Remove(item);
                    UpdateTimer();
                }
            }
        }

        private void UpdateTimer()
        {
            var nextTimeout = Items.FirstOrDefault().Key;
            if (_nextTimeout != nextTimeout)
            {
                _nextTimeout = nextTimeout;

                if (Timer == null)
                {
                    throw new ObjectDisposedException("TimeoutManager has been disposed.");
                }
                Timer.Enabled = false;
                var interval = (_nextTimeout - Timer.Now()).TotalMilliseconds;
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
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            TimeoutItems();
        }

        private void TimeoutItems()
        {
            lock (_lock)
            {
                var timeouts = Items.TakeWhile(e => e.Key <= Timer.Now()).ToList();
                foreach (var entry in timeouts)
                {
                    Items.Remove(entry.Key);
                    TimeoutsByItem.Remove(entry.Value.Item1);
                    entry.Value.Item2();
                }
                UpdateTimer();
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
    }

    /// <summary>
    /// A wrapper for <see cref="System.Timers.Timer"/> that implements <see cref="ITimer"/>.
    /// </summary>
    internal class TimerWrapper : ITimer
    {
        private readonly Timer _timer = new Timer();

        public DateTime Now() { return DateTime.UtcNow; }

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
