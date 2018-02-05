using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public class InMemoryStorage<T> : IStorage<T>
    {
        private readonly ConcurrentDictionary<string, T> _storage;

        public InMemoryStorage()
        {
            _storage = new ConcurrentDictionary<string, T>();
        }

        public async Task<T> Get(string id)
        {
            if (_storage.TryGetValue(id, out var model))
            {
                return model;
            }

            return default(T);
        }

        public async Task Set(string id, T model)
        {
            _storage.AddOrUpdate(id, model, (key, oldValue) => model);
        }

        public async Task<IEnumerable<T>> Get()
        {
            return _storage.Values;
        }

        public async Task<bool> Remove(string id)
        {
            return _storage.Remove(id, out var value);
        }
    }
}
