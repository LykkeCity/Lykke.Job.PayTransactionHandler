using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface ITransactionStateCacheManager<TItem, in TTransaction>
    {
        Task Warmup();
        Task UpdateTransactions(IEnumerable<TTransaction> transactions);
        Task ClearOutOfDate();
        Task<IEnumerable<TItem>> GetState();
    }
}
