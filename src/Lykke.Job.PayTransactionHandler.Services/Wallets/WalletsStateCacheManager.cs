﻿using System;
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
    public class WalletsStateCacheManager : StateCacheManagerBase<WalletState, PaymentBcnTransaction>
    {
        public WalletsStateCacheManager(
            ICache<WalletState> walletsCache,
            IPayInternalClient payInternalClient,
            ILog log) : base(
                walletsCache,
                payInternalClient,
                log)
        {
        }

        public override async Task WarmupAsync()
        {
            var wallets = (await PayInternalClient.GetNotExpiredWalletsAsync()).ToList();

            await Cache.AddRangeAsync(wallets.Select(x => x.ToDomain()));
        }

        public override async Task UpdateTransactionsAsync(IEnumerable<PaymentBcnTransaction> transactions)
        {
            var walletsState = (await Cache.GetAsync()).ToList();

            foreach (var trx in transactions.GroupBy(x => x.WalletAddress))
            {
                var walletState = walletsState.Single(x => x.Address == trx.Key);

                walletState.Transactions = trx;

                await Cache.UpdateAsync(walletState);

                await Log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(UpdateTransactionsAsync), $"Updated wallet {trx.Key} in cache");
            }
        }

        public override async Task ClearOutOfDateAsync()
        {
            var walletsState = (await Cache.GetAsync()).ToList();

            foreach (var walletState in walletsState.Where(x => x.DueDate <= DateTime.UtcNow))
            {
                await Cache.RemoveAsync(walletState);

                await Log.WriteInfoAsync(nameof(WalletsStateCacheManager), nameof(ClearOutOfDateAsync), $"Cleared wallet {walletState.Address} from cache with dudate = {walletState.DueDate}");
            }
        }
    }
}
