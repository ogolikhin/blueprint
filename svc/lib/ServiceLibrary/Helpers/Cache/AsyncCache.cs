using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers.Cache
{
    public class AsyncCache: IAsyncCache
    {
        private class NoCacheImpl : IAsyncCache
        {
            public Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, DateTimeOffset absoluteExpiration)
            {
                return asyncValueFactory();
            }

            public Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, TimeSpan slidingExpiration)
            {
                return asyncValueFactory();
            }

            public Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, CacheItemPolicy policy)
            {
                return asyncValueFactory();
            }

            public void Remove(string key)
            {
                // Do nothing
            }
        }

        public static IAsyncCache NoCache = new NoCacheImpl();

        public static IAsyncCache Default = new AsyncCache(MemoryCache.Default);

        protected ObjectCache Cache { get; }

        public AsyncCache(ObjectCache cache)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            Cache = cache;
        }

        public async Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, CacheItemPolicy policy)
        {
            ValidateKey(key);

            var newLazyCacheItem = new AsyncLazy<T>(asyncValueFactory);

            EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(policy);

            var existingCacheItem = Cache.AddOrGetExisting(key, newLazyCacheItem, policy);

            if (existingCacheItem != null)
            {
                return await UnwrapAsyncLazys<T>(existingCacheItem);
            }

            try
            {
                var result = newLazyCacheItem.Value;

                if (result.IsCanceled || result.IsFaulted)
                {
                    Cache.Remove(key);
                }

                return await result;
            }
            catch
            {
                Cache.Remove(key);
                throw;
            }
        }

        public Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, DateTimeOffset absoluteExpiration)
        {
            return AddOrGetExistingAsync(key, asyncValueFactory, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration });
        }

        public Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, TimeSpan slidingExpiration)
        {
            return AddOrGetExistingAsync(key, asyncValueFactory, new CacheItemPolicy { SlidingExpiration = slidingExpiration });
        }

        public void Remove(string key)
        {
            ValidateKey(key);
            Cache.Remove(key);
        }

        private static async Task<T> UnwrapAsyncLazys<T>(object item)
        {
            var asyncLazy = item as AsyncLazy<T>;
            if (asyncLazy != null)
            {
                return await asyncLazy.Value;
            }

            var task = item as Task<T>;
            if (task != null)
            {
                return await task;
            }

            var lazy = item as Lazy<T>;
            if (lazy != null)
            {
                return lazy.Value;
            }

            if (item is T)
            {
                return (T)item;
            }

            return default(T);
        }

        private static void EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(CacheItemPolicy policy)
        {
            if (policy?.RemovedCallback != null)
            {
                var originallCallback = policy.RemovedCallback;
                policy.RemovedCallback = args =>
                {
                    //unwrap the cache item in a callback given one is specified
                    var item = args?.CacheItem?.Value as AsyncLazy<T>;
                    if (item != null)
                    {
                        args.CacheItem.Value = item.IsValueCreated ? item.Value : Task.FromResult(default(T));
                    }
                    originallCallback(args);
                };
            }
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key), "Cache keys cannot be null, empty or whitespace");
            }
        }
    }
}
