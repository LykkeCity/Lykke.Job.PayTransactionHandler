using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.PayTransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using Lykke.Service.PayInternal.Client;

namespace Lykke.Job.PayTransactionHandler.Services.PayTransactions
{
    public class PayTransactionStateCacheManager : StateCacheManagerBase<PayTransactionState>
    {
        public PayTransactionStateCacheManager(
            IStateCache<PayTransactionState> payTransactionStateCache,
            IPayInternalClient payInternalClient,
            ILog log) : base(
            payTransactionStateCache,
            payInternalClient,
            log)
        {
        }

        public override Task ClearOutOfDate()
        {
            throw new NotSupportedException();
        }

        public override async Task UpdateTransactions(IEnumerable<BlockchainTransaction> transactions)
        {
            throw new NotImplementedException();
        }

        public override async Task Warmup()
        {
            await _log.WriteInfoAsync(nameof(PayTransactionStateCacheManager), nameof(Warmup), "Warming up cache with wallets");

            var payTransactions = new List<PayTransactionState>(); //await _payInternalClient.GetNotExpiredWalletsAsync();

            await _log.WriteInfoAsync(nameof(PayTransactionStateCacheManager), nameof(Warmup), $"Not expired payment transactions count = {payTransactions.Count()}");

            await _stateCache.AddRange(payTransactions);
        }
    }
}
