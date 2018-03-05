using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using Lykke.Service.PayInternal.Client;
using QBitNinja.Client;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    public class WalletsScanService : ScanServiceBase
    {
        private readonly IStateCacheManager<WalletState> _walletsStateCacheManager;
        private readonly IDiffService<BlockchainTransaction> _diffService;

        public WalletsScanService(
            IPayInternalClient payInternalClient,
            QBitNinjaClient qBitNinjaClient,
            IStateCacheManager<WalletState> walletsStateCacheManager,
            IDiffService<BlockchainTransaction> diffService,
            ILog log) : base(
                payInternalClient,
                qBitNinjaClient,
                log)
        {
            _walletsStateCacheManager = walletsStateCacheManager ??
                                        throw new ArgumentNullException(nameof(walletsStateCacheManager));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
        }

        public override async Task Execute()
        {
            //todo: move cleaning out of this service
            await _walletsStateCacheManager.ClearOutOfDate();

            var walletsInitialState = (await _walletsStateCacheManager.GetState()).ToList();

            var initialTransactions = walletsInitialState.SelectMany(x => x.Transactions);

            var currentTransactions = (await GetTransactions(walletsInitialState.Select(x => x.Address))).ToList();

            var updatedTransactions = _diffService.Diff(initialTransactions, currentTransactions);

            await Broadcast(updatedTransactions);

            await _walletsStateCacheManager.UpdateTransactions(currentTransactions);
        }
    }
}
