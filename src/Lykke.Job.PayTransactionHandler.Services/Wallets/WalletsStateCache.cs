using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    public class WalletsCache : CacheBase<WalletState>
    {
        public WalletsCache(IStorage<WalletState> storage) : base(storage)
        {
        }

        public override async Task AddAsync(WalletState item) =>
            await Storage.SetAsync(item.Address, item);

        public override async Task AddRangeAsync(IEnumerable<WalletState> items)
        {
            var tasks = items.Select(x => Storage.SetAsync(x.Address, x));

            await Task.WhenAll(tasks);
        }

        public override async Task RemoveAsync(WalletState item) =>
            await Storage.RemoveAsync(item.Address);

        public override async Task UpdateAsync(WalletState item) => 
            await Storage.SetAsync(item.Address, item);
        
    }
}
