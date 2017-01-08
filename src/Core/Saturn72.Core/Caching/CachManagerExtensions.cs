﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Saturn72.Core.Caching
{
    public static class CachManagerExtensions
    {
        /// <summary>
        /// Addsite to cache only if not already exists
        /// </summary>
        /// <param name="cacheManager"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public static void SetIfNotExists(this ICacheManager cacheManager, string key, object data)
        {
            if (cacheManager.IsSet(key))
                return;
            cacheManager.Set(key, data, 60);
        }

        /// <summary>
        /// Addsite to cache only if not already exists
        /// </summary>
        /// <param name="cacheManager"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTimeInMinutes"></param>
        public static void SetIfNotExists(this ICacheManager cacheManager, string key, object data, int cacheTimeInMinutes)
        {
            if (cacheManager.IsSet(key))
                return;
            cacheManager.Set(key, data, cacheTimeInMinutes);
        }
       
        /// <summary>
        ///     Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>Cached item</returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire)
        {
            return Get(cacheManager, key, 60, acquire);
        }

        /// <summary>
        ///     Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="key">Cache key</param>
        /// <param name="cacheTime">Cache time in minutes (0 - do not cache)</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>Cached item</returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire)
        {
            if (cacheManager.IsSet(key))
            {
                return cacheManager.Get<T>(key);
            }

            var result = acquire();
            if (cacheTime > 0)
                cacheManager.Set(key, result, cacheTime);
            return result;
        }

        /// <summary>
        ///     Removes items by pattern
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="pattern">Pattern</param>
        /// <param name="keys">All keys in the cache</param>
        public static void RemoveByPattern(this ICacheManager cacheManager, string pattern, IEnumerable<string> keys)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (var key in keys.Where(p => regex.IsMatch(p.ToString())).ToList())
                cacheManager.Remove(key);
        }
    }
}