using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.PeriodicalHandlers
{
    public class CacheOutdateHandler : TimerPeriod
    {
        private readonly ICacheMaintainer<WalletState> _walletsCache;
        private readonly ICacheMaintainer<TransactionState> _transactionsCache;
        private readonly ILog _log;

        public CacheOutdateHandler(
            ICacheMaintainer<WalletState> walletsCache,
            ICacheMaintainer<TransactionState> transactionsCache,
            TimeSpan period,
            ILog log) :
            base(nameof(CacheOutdateHandler), (int) period.TotalMilliseconds, log)
        {
            _walletsCache = walletsCache ?? throw new ArgumentNullException(nameof(walletsCache));
            _transactionsCache = transactionsCache ?? throw new ArgumentNullException(nameof(transactionsCache));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override async Task Execute()
        {
            await _walletsCache.WipeAsync();

            await _transactionsCache.WipeAsync();
        }
    }
}
