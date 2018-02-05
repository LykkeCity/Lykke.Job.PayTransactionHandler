using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public class WalletsStateCache : IWalletsStateCache
    {
        private readonly IStorage<WalletState> _storage;

        public WalletsStateCache(IStorage<WalletState> storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task Add(WalletState item)
        {
            await _storage.Set(item.Address, item);
        }

        public async Task AddRange(IEnumerable<WalletState> items)
        {
            var tasks = items.Select(x => _storage.Set(x.Address, x));

            await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<WalletState>> Get()
        {
            return await _storage.Get();
        }

        public async Task Remove(WalletState item)
        {
            await _storage.Remove(item.Address);
        }

        public async Task Update(WalletState item)
        {
            await _storage.Set(item.Address, item);
        }
    }
}
