using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonModels;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using MoreLinq;
using NBitcoin;
using QBitNinja.Client;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    /// <summary>
    /// Scans bitcoin blockchain for incoming payment transactions by wallet address
    /// </summary>
    public class WalletsScanService : ScanServiceBase<PaymentBcnTransaction>
    {
        private readonly ITransactionStateCacheManager<WalletState, PaymentBcnTransaction> _walletsStateCacheManager;
        private readonly IDiffService<PaymentBcnTransaction> _diffService;
        private readonly Network _bitcoinNetwork;

        public WalletsScanService(
            IPayInternalClient payInternalClient,
            QBitNinjaClient qBitNinjaClient,
            ITransactionStateCacheManager<WalletState, PaymentBcnTransaction> walletsStateCacheManager,
            IDiffService<PaymentBcnTransaction> diffService,
            string bitcoinNetwork,
            ILog log) : base(
                payInternalClient,
                qBitNinjaClient,
                log)
        {
            _walletsStateCacheManager = walletsStateCacheManager ??
                                        throw new ArgumentNullException(nameof(walletsStateCacheManager));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _bitcoinNetwork = Network.GetNetwork(bitcoinNetwork);
        }

        public override async Task ExecuteAsync()
        {
            //todo: move cleaning out of this service
            await _walletsStateCacheManager.ClearOutOfDateAsync();

            var walletsInitialState = (await _walletsStateCacheManager.GetStateAsync()).ToList();

            var initialTransactions = walletsInitialState.SelectMany(x => x.Transactions);

            var currentTransactions = (await GetBcnTransactionsAsync(walletsInitialState.Select(x => x.Address))).ToList();

            var updatedTransactions = _diffService.Diff(initialTransactions, currentTransactions);

            await BroadcastAsync(updatedTransactions);

            await _walletsStateCacheManager.UpdateTransactionsAsync(currentTransactions);
        }

        public override async Task CreateTransactionAsync(PaymentBcnTransaction tx)
        {
            var txDetails = await QBitNinjaClient.GetTransaction(new uint256(tx.Id));

            await PayInternalClient.CreatePaymentTransactionAsync(new CreateTransactionRequest
            {
                WalletAddress = tx.WalletAddress,
                Amount = tx.Amount,
                FirstSeen = txDetails.FirstSeen.DateTime,
                TransactionId = tx.Id,
                Confirmations = tx.Confirmations,
                BlockId = tx.BlockId,
                Blockchain = tx.Blockchain,
                AssetId = tx.AssetId,
                SourceWalletAddresses = txDetails.GetSourceWalletAddresses(_bitcoinNetwork).Select(x => x.ToString()).ToArray()
            });
        }

        public override async Task UpdateTransactionAsync(PaymentBcnTransaction tx)
        {
            await PayInternalClient.UpdateTransactionAsync(new UpdateTransactionRequest
            {
                WalletAddress = tx.WalletAddress,
                Amount = tx.Amount,
                Confirmations = tx.Confirmations,
                BlockId = tx.BlockId,
                TransactionId = tx.Id
            });
        }

        public override async Task<IEnumerable<PaymentBcnTransaction>> GetBcnTransactionsAsync(IEnumerable<string> addresses)
        {
            var balances = new List<CommonBalanceModel>();

            foreach (var batch in addresses.Batch(BatchPieceSize))
            {
                //todo: use continuation token to get full list of operations
                await Task.WhenAll(batch.Select(address => QBitNinjaClient.GetBalance(BitcoinAddress.Create(address))
                    .ContinueWith(t =>
                    {
                        lock (balances)
                        {
                            balances.Add(new CommonBalanceModel
                            {
                                WalletAddress = address,
                                Balance = t.Result
                            });
                        }
                    })));
            }

            return balances.SelectMany(x => x.GetPaymentTransactions(_bitcoinNetwork));
        }
    }
}
