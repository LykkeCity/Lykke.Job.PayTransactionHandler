using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Services.CommonServices
{
    public abstract class StateCacheManagerBase<TItem, TTransaction> : ITransactionStateCacheManager<TItem, TTransaction>
    {
        protected readonly ICache<TItem> Cache;
        protected readonly IPayInternalClient PayInternalClient;
        protected readonly ILog Log;

        protected StateCacheManagerBase(
            ICache<TItem> walletsCache,
            IPayInternalClient payInternalClient,
            ILog log)
        {
            Cache = walletsCache ?? throw new ArgumentNullException(nameof(walletsCache));
            PayInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IEnumerable<TItem>> GetState()
        {
            return await Cache.Get();
        }

        public abstract Task ClearOutOfDate();

        public abstract Task UpdateTransactions(IEnumerable<TTransaction> transactions);

        public abstract Task Warmup();
    }
}
