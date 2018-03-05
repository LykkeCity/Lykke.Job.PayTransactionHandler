using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.PayTransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using Lykke.Service.PayInternal.Client;
using QBitNinja.Client;
using System;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Services.PayTransactions
{
    public class PayTransactionScanService : ScanServiceBase
    {
        private readonly IStateCacheManager<PayTransactionState> _payTranStateCacheManager;

        public PayTransactionScanService(
            IPayInternalClient payInternalClient,
            QBitNinjaClient qBitNinjaClient,
            IStateCacheManager<PayTransactionState> payTranStateCacheManager,
            ILog log) : base(
                payInternalClient,
                qBitNinjaClient,
                log)
        {
            _payTranStateCacheManager = payTranStateCacheManager ??
                                        throw new ArgumentNullException(nameof(payTranStateCacheManager));
        }

        public override Task Execute()
        {
            // todo: implement
            throw new NotImplementedException();
        }
    }
}
