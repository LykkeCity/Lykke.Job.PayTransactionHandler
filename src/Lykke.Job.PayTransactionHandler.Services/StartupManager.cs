using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly ITransactionStateCacheManager<WalletState, PaymentBcnTransaction> _walletsStateCacheWarmer;
        private readonly ITransactionStateCacheManager<TransactionState, BcnTransaction> _transactionsStateCacheWarmer;
        private readonly ILog _log;

        public StartupManager(
            ITransactionStateCacheManager<WalletState, PaymentBcnTransaction> walletsStateCacheWarmer, 
            ITransactionStateCacheManager<TransactionState, BcnTransaction> transactionsStateCacheWarmer,
            ILog log)
        {
            _walletsStateCacheWarmer = walletsStateCacheWarmer;
            _transactionsStateCacheWarmer = transactionsStateCacheWarmer;
            _log = log;
        }

        public async Task StartAsync()
        {
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Warming up wallets cache ...");

            await _walletsStateCacheWarmer.WarmupAsync();

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Done.");

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Warming up transactions cache ...");

            await _transactionsStateCacheWarmer.WarmupAsync();

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "Done.");
        }
    }
}
