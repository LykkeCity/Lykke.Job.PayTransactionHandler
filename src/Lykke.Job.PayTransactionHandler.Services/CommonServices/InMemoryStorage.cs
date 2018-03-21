﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services.CommonServices
{
    public class InMemoryStorage<T> : IStorage<T>
    {
        private readonly ConcurrentDictionary<string, T> _storage;

        public InMemoryStorage() =>
            _storage = new ConcurrentDictionary<string, T>();

        [SuppressMessage("Compiler", "CS1998")]
        public async Task<T> GetAsync(string id)
        {
            if (_storage.TryGetValue(id, out var model))
            {
                return model;
            }

            return default(T);
        }

        [SuppressMessage("Compiler", "CS1998")]
        public async Task SetAsync(string id, T model) =>
            _storage.AddOrUpdate(id, model, (key, oldValue) => model);

        [SuppressMessage("Compiler", "CS1998")]
        public async Task<IEnumerable<T>> GetAsync() =>
            _storage.Values;

        [SuppressMessage("Compiler", "CS1998")]
        public async Task<bool> RemoveAsync(string id) =>
            _storage.Remove(id, out var value);
    }
}
