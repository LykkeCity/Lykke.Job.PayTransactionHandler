using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using Lykke.Job.PayTransactionHandler.Services.Extensions;
using Microsoft.Extensions.Caching.Memory;
using BlockchainType = Lykke.Job.PayTransactionHandler.Core.BlockchainType;

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

        public async Task WarmUpAsync()
        {
            IEnumerable<WalletStateResponse> walletsState = await _payInternalClient.GetNotExpiredWalletsAsync();

            foreach (var walletStateResponse in walletsState)
            {
                WalletState walletState = walletStateResponse.ToDomain();

                await _cache.SetWithPartitionAsync(CachePartitionName, walletState.Address, walletState);
            }
        }

        public async Task WipeAsync()
        {
            IEnumerable<WalletState> cached = await _cache.GetPartitionAsync<WalletState>(CachePartitionName);

            foreach (var walletState in cached.Where(x => x.DueDate <= DateTime.UtcNow))
            {
                BlockchainType blockchain = walletState.Transactions.Select(x => x.Blockchain).Single();

                await _payInternalClient.SetWalletExpiredAsync(new BlockchainWalletExpiredRequest
                {
                    WalletAddress = walletState.Address,
                    Blockchain = Enum.Parse<Service.PayInternal.Client.Models.BlockchainType>(blockchain.ToString())
                });

                await _cache.RemoveWithPartitionAsync(CachePartitionName, walletState.Address);

                await _log.WriteInfoAsync(nameof(WalletsStateCacheMaintainer), nameof(WipeAsync),
                    $"Cleared wallet {walletState.Address} from cache with dudate = {walletState.DueDate}");
            }
        }

        public async Task SetItemAsync(WalletState item)
        {
            await _cache.SetWithPartitionAsync(CachePartitionName, item.Address, item);

            await _log.WriteInfoAsync(nameof(WalletsStateCacheMaintainer), nameof(SetItemAsync), $"Updated wallet {item.Address} in cache");
        }

        public async Task<IEnumerable<WalletState>> GetAsync()
        {
            return await _cache.GetPartitionAsync<WalletState>(CachePartitionName);
        }
    }
}
