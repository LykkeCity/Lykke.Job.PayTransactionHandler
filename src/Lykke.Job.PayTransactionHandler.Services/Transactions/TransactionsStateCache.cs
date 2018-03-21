using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;

namespace Lykke.Job.PayTransactionHandler.Services.Transactions
{
    public class TransactionsCache : CacheBase<TransactionState>
    {
        public TransactionsCache(IStorage<TransactionState> storage) : base(storage)
        {
        }

        public override async Task AddAsync(TransactionState item) =>
            await Storage.SetAsync(item.Transaction.Id, item);

        public override async Task AddRangeAsync(IEnumerable<TransactionState> items)
        {
            var tasks = items.Select(x => Storage.SetAsync(x.Transaction.Id, x));

            await Task.WhenAll(tasks);
        }

        public override async Task RemoveAsync(TransactionState item) =>
            await Storage.RemoveAsync(item.Transaction.Id);

        public override async Task UpdateAsync(TransactionState item) =>
            await Storage.SetAsync(item.Transaction.Id, item);
    }
}
