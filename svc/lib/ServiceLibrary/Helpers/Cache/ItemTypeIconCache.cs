using System;
using System.Globalization;
using System.Runtime.Caching;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Helpers.Cache
{
    public sealed class ItemTypeIconCache : IItemTypeIconCache
    {
        public readonly static IItemTypeIconCache Instance = new ItemTypeIconCache();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ItemTypeIconCache()
        {
        }

        private readonly MemoryCache _cache;
        private readonly DateTimeOffset _defaultExpirationOffset = DateTimeOffset.UtcNow.AddDays(1);

        internal ItemTypeIconCache()
        {
            _cache = new MemoryCache("ItemTypeIconCache");
        }

        public void AddOrReplace(IconType predefined, int? itemTypeId, string hexColor, byte[] data)
        {
            var key = String.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", predefined.ToString(), itemTypeId.GetValueOrDefault().ToString(), hexColor);

            if (!_cache.Contains(key))
            {
                _cache.Add(key, data, _defaultExpirationOffset);
            }
            else
            {
                _cache.Set(key, data, _defaultExpirationOffset);
            }
        }

        public byte[] GetValue(IconType predefined, int? itemTypeId, string hexColor)
        {
            var key = String.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", predefined.ToString(), itemTypeId.GetValueOrDefault().ToString(), hexColor);

            var cacheItem = _cache.Get(key);
            return (byte[])cacheItem;
        }

        public void Remove(IconType predefined, int? itemTypeId, string hexColor)
        {
            var key = String.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", predefined.ToString(), itemTypeId.GetValueOrDefault().ToString(), hexColor);

            _cache.Remove(key);
        }
    }
}
