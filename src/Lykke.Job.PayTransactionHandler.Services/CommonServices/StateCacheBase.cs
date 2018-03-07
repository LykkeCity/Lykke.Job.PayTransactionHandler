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

        public async Task<IEnumerable<T>> Get() =>
            await Storage.Get();

        public abstract Task Add(T item);

        public abstract Task AddRange(IEnumerable<T> items);

        public abstract Task Remove(T item);

        public abstract Task Update(T item);
    }
}
