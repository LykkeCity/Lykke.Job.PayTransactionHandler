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

        public override async Task Add(TransactionState item) =>
            await Storage.Set(item.Transaction.Id, item);

        public override async Task AddRange(IEnumerable<TransactionState> items)
        {
            var tasks = items.Select(x => Storage.Set(x.Transaction.Id, x));

            await Task.WhenAll(tasks);
        }

        public override async Task Remove(TransactionState item) =>
            await Storage.Remove(item.Transaction.Id);

        public override async Task Update(TransactionState item) =>
            await Storage.Set(item.Transaction.Id, item);
    }
}
