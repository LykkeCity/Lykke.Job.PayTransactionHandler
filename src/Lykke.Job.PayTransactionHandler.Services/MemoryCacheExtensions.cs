using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public static class MemoryCacheExtensions
    {
        private static readonly IReadOnlyDictionary<int, SemaphoreSlim> Locks;

        static MemoryCacheExtensions()
        {
            Locks = Enumerable.Range(0, 1000).ToDictionary(
                i => i,
                i => new SemaphoreSlim(1, 1));
        }

        private static async Task<SemaphoreSlim> GetLockAsync(string key)
        {
            int hash = key.GetHashCode();

            int number = Math.Abs(hash % Locks.Count);

            SemaphoreSlim semaphore = Locks[number];

            await semaphore.WaitAsync();

            return semaphore;
        }

        public static async Task<TItem> SetWithPartitionAsync<TItem>(this IMemoryCache cache, object partitionKey, object key, TItem value)
        {
            SemaphoreSlim semaphore = await GetLockAsync(key.ToString());

            TItem result;

            try
            {
                result = cache.Set(key, value);

                SemaphoreSlim partitionSemaphore = await GetLockAsync(partitionKey.ToString());

                try
                {
                    var partition = cache.Get<List<object>>(partitionKey);

                    if (partition == null)
                    {
                        partition = new List<object>();
                    }

                    if (!partition.Contains(key))
                    {
                        partition.Add(key);
                    }

                    cache.Set(partitionKey, partition);
                }
                finally
                {
                    partitionSemaphore.Release();
                }
            }
            finally
            {
                semaphore.Release();
            }

            return result;
        }

        public static async Task RemoveWithPartitionAsync(this IMemoryCache cache, object partitionKey, object key)
        {
            SemaphoreSlim semaphore = await GetLockAsync(key.ToString());

            try
            {
                cache.Remove(key);

                SemaphoreSlim partitionSemaphore = await GetLockAsync(partitionKey.ToString());

                try
                {
                    if (cache.TryGetValue(partitionKey, out List<object> partition))
                    {
                        if (partition.Remove(key))
                        {
                            cache.Set(partitionKey, partition);
                        }
                    }
                }
                finally
                {
                    partitionSemaphore.Release();
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task<IEnumerable<TItem>> GetPartitionAsync<TItem>(this IMemoryCache cache, object partitionKey)
        {
            SemaphoreSlim partitionSemaphore = await GetLockAsync(partitionKey.ToString());

            List<object> partitionKeys;

            try
            {
                partitionKeys = cache.Get<List<object>>(partitionKey);
            }
            finally
            {
                partitionSemaphore.Release();
            }

            var items = new List<TItem>();

            if (partitionKeys != null && partitionKeys.Any())
            {
                foreach (var partitionItemKey in partitionKeys)
                {
                    SemaphoreSlim semaphore = await GetLockAsync(partitionItemKey.ToString());

                    try
                    {
                        if (cache.TryGetValue(partitionItemKey, out TItem item))
                        {
                            items.Add(item);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
            }

            return items;
        }
    }
}
