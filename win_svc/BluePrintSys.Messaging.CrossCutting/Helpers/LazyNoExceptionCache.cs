using BluePrintSys.Messaging.CrossCutting.Logging;
using System;

namespace BluePrintSys.Messaging.CrossCutting.Helpers
{
    public class LazyNoExceptionCache<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly Action<T> _destroyAction;
        private T _instance;
        private readonly object _lock = new object();

        public LazyNoExceptionCache(Func<T> newValueFactory, Action<T> destroyValueAction = null)
        {
            if (newValueFactory == null)
            {
                throw new ArgumentNullException(nameof(newValueFactory));
            }
            _factory = newValueFactory;
            _destroyAction = destroyValueAction;
        }

        public T Value
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var instance = _factory.Invoke();
                        _instance = instance;
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Destroy the previous value if already created (move to `value is not created` phase)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public void Reset()
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    try
                    {
                        if (_destroyAction != null)
                        {
                            _destroyAction.Invoke(_instance);
                        }
                        else
                        {
                            Log.Debug($"LazyNoExceptionCache: {typeof(T).Name} - Destroy value action is not defined");
                        }
                    }
                    finally
                    {
                        _instance = null;
                    }
                }
            }
        }

        public bool IsValueCreated
        {
            get
            {
                lock (_lock)
                {
                    return _instance != null;
                }
            }
        }
    }
}
