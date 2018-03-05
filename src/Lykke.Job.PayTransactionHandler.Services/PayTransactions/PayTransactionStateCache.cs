using Lykke.Job.PayTransactionHandler.Core.Domain.PayTransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Services.PayTransactions
{
    public class PayTransactionStateCache : StateCacheBase<PayTransactionState>
    {
        public PayTransactionStateCache(IStorage<PayTransactionState> storage) : base(storage)
        {
        }

        public override async Task Add(PayTransactionState item) =>
            await _storage.Set(item.TransactionId, item);

        public override async Task AddRange(IEnumerable<PayTransactionState> items)
        {
            var tasks = items.Select(x => _storage.Set(x.TransactionId, x));

            await Task.WhenAll(tasks);
        }

        public override async Task Remove(PayTransactionState item) =>
            await _storage.Remove(item.TransactionId);

        public override async Task Update(PayTransactionState item) =>
            await _storage.Set(item.TransactionId, item);
    }
}
