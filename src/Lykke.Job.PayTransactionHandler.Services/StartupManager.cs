using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly ICacheMaintainer<WalletState> _walletsStateCacheWarmer;
        private readonly ICacheMaintainer<TransactionState> _transactionsStateCacheWarmer;
        private readonly ILog _log;

        public StartupManager(
            ICacheMaintainer<WalletState> walletsStateCacheWarmer,
            ICacheMaintainer<TransactionState> transactionsStateCacheWarmer,
            ILogFactory logFactory)
        {
            _walletsStateCacheWarmer = walletsStateCacheWarmer;
            _transactionsStateCacheWarmer = transactionsStateCacheWarmer;
            _log = logFactory.CreateLog(this);
        }

        public async Task StartAsync()
        {
            _log.Info("Warming up wallets cache ...");

            await _walletsStateCacheWarmer.WarmUpAsync();

            _log.Info("Done.");

            _log.Info("Warming up transactions cache ...");

            await _transactionsStateCacheWarmer.WarmUpAsync();

            _log.Info("Done.");
        }
    }
}
