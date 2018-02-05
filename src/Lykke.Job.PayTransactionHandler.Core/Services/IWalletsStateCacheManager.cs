using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IWalletsStateCacheManager
    {
        Task Warmup();
        Task UpdateTransactions(IEnumerable<BlockchainTransaction> transactions);
        Task ClearOutOfDate();
        Task<IEnumerable<WalletState>> GetState();
    }
}
