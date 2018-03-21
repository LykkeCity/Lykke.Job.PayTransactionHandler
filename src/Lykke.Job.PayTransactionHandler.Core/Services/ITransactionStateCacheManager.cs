using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface ITransactionStateCacheManager<TItem, in TTransaction>
    {
        Task WarmupAsync();
        Task UpdateTransactionsAsync(IEnumerable<TTransaction> transactions);
        Task ClearOutOfDateAsync();
        Task<IEnumerable<TItem>> GetStateAsync();
    }
}
