using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.PeriodicalHandlers
{
    public class TransactionsScanHandler : TimerPeriod
    {
        private readonly IBlockchainScanerProvider _scanServiceProvider;
        private readonly ILog _log;

        public TransactionsScanHandler(
            IBlockchainScanerProvider scanServiceProvider,
            TimeSpan period,
            ILog log) :
            base(nameof(TransactionsScanHandler), (int)period.TotalMilliseconds, log)
        {
            _scanServiceProvider = scanServiceProvider ?? throw new ArgumentNullException(nameof(scanServiceProvider));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override async Task Execute()
        {
            IScanService scanService = _scanServiceProvider.Get(BlockchainScanType.Transaction.ToString());

            await scanService.Execute();
        }
    }
}
