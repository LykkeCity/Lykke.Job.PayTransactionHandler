﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    public class WalletsStateCacheMaintainer : ICacheMaintainer<WalletState>
    {
        //todo: replace with IDistributedCache 
        private readonly IMemoryCache _cache;
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;

        private const string CachePartitionName = "WalletsState";

        public WalletsStateCacheMaintainer(
            IMemoryCache cache,
            IPayInternalClient payInternalClient,
            ILog log)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task WarmUp()
        {
            IEnumerable<WalletStateResponse> walletsState = await _payInternalClient.GetNotExpiredWalletsAsync();

            foreach (var walletStateResponse in walletsState)
            {
                WalletState walletState = walletStateResponse.ToDomain();

                await _cache.SetWithPartitionAsync(CachePartitionName, walletState.Address, walletState);
            }
        }

        public async Task Wipe()
        {
            IEnumerable<WalletState> cached = await _cache.GetPartitionAsync<WalletState>(CachePartitionName);

            foreach (var walletState in cached.Where(x => x.DueDate <= DateTime.UtcNow))
            {
                await _cache.RemoveWithPartitionAsync(CachePartitionName, walletState.Address);

                await _log.WriteInfoAsync(nameof(WalletsStateCacheMaintainer), nameof(Wipe),
                    $"Cleared wallet {walletState.Address} from cache with dudate = {walletState.DueDate}");
            }
        }

        public async Task UpdateItem(WalletState item)
        {
            await _cache.SetWithPartitionAsync(CachePartitionName, item.Address, item);

            await _log.WriteInfoAsync(nameof(WalletsStateCacheMaintainer), nameof(UpdateItem), $"Updated wallet {item.Address} in cache");
        }

        public async Task<IEnumerable<WalletState>> Get()
        {
            return await _cache.GetPartitionAsync<WalletState>(CachePartitionName);
        }
    }
}
