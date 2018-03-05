using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Services.CommonServices
{
    public abstract class StateCacheManagerBase<T> : IStateCacheManager<T>
    {
        protected readonly IStateCache<T> _stateCache;
        protected readonly IPayInternalClient _payInternalClient;
        protected readonly ILog _log;

        public StateCacheManagerBase(
            IStateCache<T> walletsStateCache,
            IPayInternalClient payInternalClient,
            ILog log)
        {
            _stateCache = walletsStateCache ?? throw new ArgumentNullException(nameof(walletsStateCache));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }        

        public async Task<IEnumerable<T>> GetState() =>
            await _stateCache.Get();

        public abstract Task ClearOutOfDate();

        public abstract Task UpdateTransactions(IEnumerable<BlockchainTransaction> transactions);

        public abstract Task Warmup();
    }
}
