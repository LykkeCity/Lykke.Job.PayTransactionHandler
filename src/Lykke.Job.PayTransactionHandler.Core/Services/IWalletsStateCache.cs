using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IWalletsStateCache
    {
        Task Add(WalletState item);
        Task AddRange(IEnumerable<WalletState> items);
        Task<IEnumerable<WalletState>> Get();
        Task Remove(WalletState item);
        Task Update(WalletState item);
    }
}
