using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using Lykke.Service.PayInternal.Client;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    public class WalletsStateCacheManager : StateCacheManagerBase<WalletState>
    {
        public WalletsStateCacheManager(
            IStateCache<WalletState> walletsStateCache,
            IPayInternalClient payInternalClient,
            ILog log) : base(
                walletsStateCache,
                payInternalClient,
                log)
        {
        }

        public override async Task Warmup()
        {
            await _log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(Warmup), "Warming up cache with wallets");

            var wallets = await _payInternalClient.GetNotExpiredWalletsAsync();

            await _log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(Warmup), $"Not expired wallets count = {wallets.Count()}");

            await _stateCache.AddRange(wallets.Select(x => x.ToDomain()));
        }

        public override async Task UpdateTransactions(IEnumerable<BlockchainTransaction> transactions)
        {
            var walletsState = (await _stateCache.Get()).ToList();

            foreach (var trx in transactions.GroupBy(x => x.WalletAddress))
            {
                var walletState = walletsState.Single(x => x.Address == trx.Key);

                walletState.Transactions = trx;

                await _stateCache.Update(walletState);

                await _log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(UpdateTransactions), $"Updated wallet {trx.Key} in cache");
            }
        }

        public override async Task ClearOutOfDate()
        {
            var walletsState = (await _stateCache.Get()).ToList();

            foreach (var walletState in walletsState.Where(x => x.DueDate <= DateTime.UtcNow))
            {
                await _stateCache.Remove(walletState);

                await _log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(ClearOutOfDate), $"Cleared wallet {walletState.Address} from cache with dudate = {walletState.DueDate}");
            }
        }
    }
}
