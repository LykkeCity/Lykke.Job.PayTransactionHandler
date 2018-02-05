using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models;
using MoreLinq;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public class WalletsScanService : IWalletsScanService
    {
        private class WalletBalanceModel
        {
            public string WalletAddress { get; set; }
            public BalanceModel Balance { get; set; }

            public IEnumerable<BlockchainTransaction> GetTransactions()
            {
                return Balance?.Operations?.Select(x => new BlockchainTransaction
                {
                    WalletAddress = WalletAddress,
                    Amount = (double) x.Amount.ToDecimal(MoneyUnit.Satoshi),
                    AssetId = nameof(MoneyUnit.Satoshi),
                    Blockchain = "Bitcoin",
                    Id = x.TransactionId.ToString(),
                    BlockId = x.BlockId?.ToString(),
                    Confirmations = x.Confirmations
                });
            }
        }

        private readonly IPayInternalClient _payInternalClient;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly IWalletsStateCacheManager _walletsStateCacheManager;
        private readonly IDiffService<BlockchainTransaction> _diffService;
        private readonly ILog _log;

        private const int BatchPieceSize = 15;

        public WalletsScanService(
            IPayInternalClient payInternalClient,
            QBitNinjaClient qBitNinjaClient,
            IWalletsStateCacheManager walletsStateCacheManager,
            IDiffService<BlockchainTransaction> diffService,
            ILog log)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _walletsStateCacheManager = walletsStateCacheManager ??
                                        throw new ArgumentNullException(nameof(walletsStateCacheManager));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task Execute()
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

        private async Task Broadcast(IEnumerable<DiffResult<BlockchainTransaction>> updated)
        {
            foreach (var diffResult in updated)
            {
                var tx = diffResult.Object;

                switch (diffResult.CompareState)
                {
                    case DiffState.New:

                        var txDetails = await _qBitNinjaClient.GetTransaction(new uint256(tx.Id));

                        await _payInternalClient.CreateTransaction(new CreateTransactionRequest
                        {
                            WalletAddress = tx.WalletAddress,
                            Amount = tx.Amount,
                            FirstSeen = txDetails.FirstSeen.DateTime,
                            TransactionId = tx.Id,
                            Confirmations = tx.Confirmations,
                            BlockId = tx.BlockId,
                            Blockchain = tx.Blockchain,
                            AssetId = tx.AssetId
                        });

                        break;

                    case DiffState.Updated:

                        await _payInternalClient.UpdateTransaction(new UpdateTransactionRequest
                        {
                            WalletAddress = tx.WalletAddress,
                            Amount = tx.Amount,
                            Confirmations = tx.Confirmations,
                            BlockId = tx.BlockId,
                            TransactionId = tx.Id
                        });

                        break;

                    default: throw new Exception("Unknown transactions diff state");
                }
            }
        }

        private async Task<IEnumerable<BlockchainTransaction>> GetTransactions(IEnumerable<string> addresses)
        {
            var balances = new List<WalletBalanceModel>();

            foreach (var batch in addresses.Batch(BatchPieceSize))
            {
                //todo: use continuation token to get full list of operations
                await Task.WhenAll(batch.Select(address => _qBitNinjaClient.GetBalance(BitcoinAddress.Create(address))
                    .ContinueWith(t =>
                    {
                        lock (balances)
                        {
                            balances.Add(new WalletBalanceModel
                            {
                                WalletAddress = address,
                                Balance = t.Result
                            });
                        }
                    })));
            }

            return balances.SelectMany(x => x.GetTransactions());
        }
    }
}
