using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public class WalletsStateCacheManager : IWalletsStateCacheManager
    {
        private readonly IWalletsStateCache _walletsStateCache;
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;


        public WalletsStateCacheManager(
            IWalletsStateCache walletsStateCache,
            IPayInternalClient payInternalClient,
            ILog log)
        {
            _walletsStateCache = walletsStateCache ?? throw new ArgumentNullException(nameof(walletsStateCache));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task Warmup()
        {
            await _log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(Warmup), "Warming up cache with wallets");

            var wallets = await _payInternalClient.GetNotExpiredWalletsAsync();

            await _log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(Warmup), $"Not expired wallets count = {wallets.Count()}");

            await _walletsStateCache.AddRange(wallets.Select(x => x.ToDomain()));
        }

        public async Task UpdateTransactions(IEnumerable<BlockchainTransaction> transactions)
        {
            var walletsState = (await _walletsStateCache.Get()).ToList();

            foreach (var trx in transactions.GroupBy(x => x.WalletAddress))
            {
                var walletState = walletsState.Single(x => x.Address == trx.Key);

                walletState.Transactions = trx;

                await _walletsStateCache.Update(walletState);

                await _log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(UpdateTransactions), $"Updated wallet {trx.Key} in cache");
            }
        }

        public async Task ClearOutOfDate()
        {
            var walletsState = (await _walletsStateCache.Get()).ToList();

            foreach (var walletState in walletsState.Where(x => x.DueDate <= DateTime.UtcNow))
            {
                await _walletsStateCache.Remove(walletState);

                await _log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(ClearOutOfDate), $"Cleared wallet {walletState.Address} from cache with dudate = {walletState.DueDate}");
            }
        }

        public async Task<IEnumerable<WalletState>> GetState()
        {
            return await _walletsStateCache.Get();
        }
    }
}
