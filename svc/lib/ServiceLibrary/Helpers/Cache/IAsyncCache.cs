﻿using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers.Cache
{
    public interface IAsyncCache
    {
        void Remove(string key);

        Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, CacheItemPolicy policy);

        Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, DateTimeOffset absoluteExpiration);

        Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> asyncValueFactory, TimeSpan slidingExpiration);
    }
}
