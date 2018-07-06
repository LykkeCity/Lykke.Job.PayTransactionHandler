using System.Threading.Tasks;
using Autofac;
using Common.Log;
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
            ILog log)
        {
            _walletsStateCacheWarmer = walletsStateCacheWarmer;
            _transactionsStateCacheWarmer = transactionsStateCacheWarmer;
            _log = log;
        }

        public async Task StartAsync()
        {
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Warming up wallets cache ...");

            await _walletsStateCacheWarmer.WarmUpAsync();

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Done.");

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Warming up transactions cache ...");

            await _transactionsStateCacheWarmer.WarmUpAsync();

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Done.");
        }
    }
}
