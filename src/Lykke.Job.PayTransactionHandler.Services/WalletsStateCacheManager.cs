using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public class WalletsStateCacheManager : IWalletsStateCacheManager
    {
        private readonly IWalletsStateCache _walletsStateCache;
        private readonly IPayInternalClient _payInternalClient;


        public WalletsStateCacheManager(
            IWalletsStateCache walletsStateCache,
            IPayInternalClient payInternalClient)
        {
            _walletsStateCache = walletsStateCache ?? throw new ArgumentNullException(nameof(walletsStateCache));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
        }

        public async Task Warmup()
        {
            var wallets = await _payInternalClient.GetNotExpiredWalletsAsync();

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
            }
        }

        public async Task ClearOutOfDate()
        {
            var walletsState = (await _walletsStateCache.Get()).ToList();

            foreach (var walletState in walletsState.Where(x => x.DueDate <= DateTime.UtcNow))
            {
                await _walletsStateCache.Remove(walletState);
            }
        }

        public async Task<IEnumerable<WalletState>> GetState()
        {
            return await _walletsStateCache.Get();
        }
    }
}
