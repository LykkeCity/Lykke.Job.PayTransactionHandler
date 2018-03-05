using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IStateCacheManager<T>
    {
        Task Warmup();
        Task UpdateTransactions(IEnumerable<BlockchainTransaction> transactions);
        Task ClearOutOfDate();
        Task<IEnumerable<T>> GetState();
    }
}
