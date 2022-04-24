using Ardalis.GuardClauses;
using Pl.Sas.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Pl.Sas.Infrastructure.Caching
{
    public class MemoryCacheService : IMemoryCacheService
    {
        #region Properties And Constructor

        private readonly IMemoryCache _memoryCache;

        private static readonly ConcurrentDictionary<string, bool> allKeys = new();

        private static CancellationTokenSource _cancellationTokenSource = new();

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            Guard.Against.Null(memoryCache, nameof(memoryCache));
            _memoryCache = memoryCache;
        }

        #endregion Properties And Constructor

        #region Utilities

        protected MemoryCacheEntryOptions GetMemoryCacheEntryOptions(int cacheTime = 60)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions()
                .AddExpirationToken(new CancellationChangeToken(_cancellationTokenSource.Token))
                .RegisterPostEvictionCallback(PostEviction);
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheTime);
            return options;
        }

        private static string AddKey(string key)
        {
            allKeys.TryAdd(key, true);
            return key;
        }

        private static string RemoveKey(string key)
        {
            TryRemoveKey(key);
            return key;
        }

        private static void TryRemoveKey(string key)
        {
            if (!allKeys.TryRemove(key, out _))
            {
                allKeys.TryUpdate(key, false, false);
            }
        }

        private static void ClearKeys()
        {
            foreach (string key in allKeys.Where(p => !p.Value).Select(p => p.Key).ToList())
            {
                RemoveKey(key);
            }
        }

        private void PostEviction(object key, object value, EvictionReason reason, object state)
        {
            if (reason == EvictionReason.Replaced)
            {
                return;
            }

            ClearKeys();

            TryRemoveKey(key?.ToString() ?? "");
        }

        #endregion Utilities

        #region Get Method

        public virtual TItem GetByKey<TItem>(string key)
        {
            return _memoryCache.Get<TItem>(key);
        }

        public virtual TItem GetOrCreate<TItem>(string key, Func<TItem> factory)
        {
            Guard.Against.Null(factory, nameof(factory));

            return _memoryCache.GetOrCreate(AddKey(key), entry =>
            {
                return factory();
            });
        }

        public virtual async Task<TItem> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory)
        {
            Guard.Against.Null(factory, nameof(factory));

            return await _memoryCache.GetOrCreateAsync(AddKey(key), async entry =>
            {
                return await factory();
            });
        }

        public virtual TItem GetOrCreate<TItem>(string key, Func<TItem> factory, int time)
        {
            Guard.Against.Null(factory, nameof(factory));

            return _memoryCache.GetOrCreate(AddKey(key), entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(time);
                return factory();
            });
        }

        public virtual async Task<TItem> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory, int time)
        {
            Guard.Against.Null(factory, nameof(factory));

            return await _memoryCache.GetOrCreateAsync(AddKey(key), async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(time);
                return await factory();
            });
        }

        public virtual async Task<DateTime> GetTime()
        {
            return await Task.FromResult(DateTime.Now);
        }

        #endregion Get Method

        #region Set Method

        public virtual TItem SetValue<TItem>(string key, TItem value)
        {
            return _memoryCache.Set(AddKey(key), value, GetMemoryCacheEntryOptions());
        }

        public virtual TItem SetValue<TItem>(string key, TItem value, int time)
        {
            return _memoryCache.Set(AddKey(key), value, GetMemoryCacheEntryOptions(time));
        }

        #endregion Set Method

        #region Remove Method

        public virtual void Remove(string key)
        {
            _memoryCache.Remove(RemoveKey(key));
        }

        public virtual void RemoveByPrefix(string pattern)
        {
            Regex regex = new(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            List<string> matchesKeys = allKeys.Where(key => regex.IsMatch(key.Key)).Select(q => q.Key).ToList();
            matchesKeys.ForEach(key => Remove(key));
        }

        public virtual void Clear()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion Remove Method
    }
}