using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    public class WalletsStateCache : StateCacheBase<WalletState>
    {
        public WalletsStateCache(IStorage<WalletState> storage) : base(storage)
        {            
        }

        public override async Task Add(WalletState item) =>
            await _storage.Set(item.Address, item);

        public override async Task AddRange(IEnumerable<WalletState> items)
        {
            var tasks = items.Select(x => _storage.Set(x.Address, x));

            await Task.WhenAll(tasks);
        }

        public override async Task Remove(WalletState item) =>
            await _storage.Remove(item.Address);

        public override async Task Update(WalletState item) => 
            await _storage.Set(item.Address, item);
    }
}
