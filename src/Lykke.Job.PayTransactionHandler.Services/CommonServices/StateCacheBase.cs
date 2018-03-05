using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services.CommonServices
{
    public abstract class StateCacheBase<T> : IStateCache<T>
    {
        protected readonly IStorage<T> _storage;

        public StateCacheBase(IStorage<T> storage) =>
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

        public async Task<IEnumerable<T>> Get() =>
            await _storage.Get();

        public abstract Task Add(T item);

        public abstract Task AddRange(IEnumerable<T> items);

        public abstract Task Remove(T item);

        public abstract Task Update(T item);
    }
}
