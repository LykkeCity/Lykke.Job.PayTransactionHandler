using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
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
            ILogFactory logFactory) : base(period, logFactory)
        {
            _scanServiceProvider = scanServiceProvider ?? throw new ArgumentNullException(nameof(scanServiceProvider));
            _log = logFactory.CreateLog(this);
        }

        public override async Task Execute()
        {
            IScanService scanService = _scanServiceProvider.Get(BlockchainScanType.Transaction);

            await scanService.ExecuteAsync();
        }
    }
}
