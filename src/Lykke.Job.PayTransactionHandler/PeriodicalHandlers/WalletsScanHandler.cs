using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.PeriodicalHandlers
{
    public class WalletsScanHandler : TimerPeriod
    {
        private readonly IScanService _walletsScanService;
        private readonly ILog _log;

        public WalletsScanHandler(
            IScanService walletsScanService,
            TimeSpan period,
            ILog log) :
            base(nameof(WalletsScanHandler), (int)period.TotalMilliseconds, log)
        {
            _walletsScanService = walletsScanService ?? throw new ArgumentNullException(nameof(walletsScanService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override async Task Execute()
        {
            await _walletsScanService.Execute();
        }
    }
}
