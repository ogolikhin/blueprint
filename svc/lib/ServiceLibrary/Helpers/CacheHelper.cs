using System;

namespace ServiceLibrary.Helpers
{
    public class CacheHelper<T>
    {
        private readonly object _cacheLock = new object();
        private readonly TimeSpan _expirationTime;

        private readonly Func<DateTime> _currentTimeFactory;
        private DateTime? _lastUpdateTime; // also indicates that value was not initialized first

        private readonly Func<T> _valueFactory;
        private T _value;

        public CacheHelper(TimeSpan expirationTime, Func<T> valueFactory, Func<DateTime> currentTimeFactory = null)
        {
            if (expirationTime == null || expirationTime.TotalMilliseconds <= 0)
            {
                throw new ArgumentException("Argument should be > 0", nameof(expirationTime));
            }
            _expirationTime = expirationTime;

            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }
            _valueFactory = valueFactory;

            _currentTimeFactory = currentTimeFactory ?? (() => DateTime.Now);
        }

        private bool TryGetCachedValue(out T value)
        {
            var v = _value;
            var canUseCachedValue = _lastUpdateTime.HasValue && (_currentTimeFactory.Invoke() - _lastUpdateTime) < _expirationTime;

            value = canUseCachedValue ? v : default(T);

            return canUseCachedValue;
        }

        public T Get()
        {
            try
            {
                T cachedValue;
                if (TryGetCachedValue(out cachedValue))
                {
                    return cachedValue;
                }

                lock (_cacheLock)
                {
                    if (TryGetCachedValue(out cachedValue))
                    {
                        return cachedValue;
                    }

                    var newValue = _valueFactory();
                    _value = newValue;
                    _lastUpdateTime = _currentTimeFactory.Invoke();

                    return newValue;
                }
            }
            catch
            {
                var newValue = _valueFactory();
                return newValue;
            }
        }
    }
}
