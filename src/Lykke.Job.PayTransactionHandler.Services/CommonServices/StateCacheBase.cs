using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Job.PayTransactionHandler.Services.CommonServices
{
    public abstract class CacheBase<T> : ICache<T>
    {
        protected readonly IStorage<T> Storage;

        protected CacheBase(IStorage<T> storage)
        {
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<IEnumerable<T>> GetAsync() =>
            await Storage.GetAsync();

        public abstract Task AddAsync(T item);

        public abstract Task AddRangeAsync(IEnumerable<T> items);

        public abstract Task RemoveAsync(T item);

        public abstract Task UpdateAsync(T item);
    }
}
