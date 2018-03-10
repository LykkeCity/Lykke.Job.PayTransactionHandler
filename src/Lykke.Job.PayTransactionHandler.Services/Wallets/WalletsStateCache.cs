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

        public override async Task Add(WalletState item) =>
            await Storage.Set(item.Address, item);

        public override async Task AddRange(IEnumerable<WalletState> items)
        {
            var tasks = items.Select(x => Storage.Set(x.Address, x));

            await Task.WhenAll(tasks);
        }

        public override async Task Remove(WalletState item) =>
            await Storage.Remove(item.Address);

        public override async Task Update(WalletState item) => 
            await Storage.Set(item.Address, item);
        
    }
}
