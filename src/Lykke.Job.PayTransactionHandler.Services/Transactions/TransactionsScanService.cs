using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using Lykke.Service.PayInternal.Client;
using QBitNinja.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using NBitcoin;
using QBitNinja.Client.Models;
using MoreLinq;

namespace Lykke.Job.PayTransactionHandler.Services.Transactions
{
    public class TransactionsScanService : ScanServiceBase<BcnTransaction>
    {
        private readonly ITransactionStateCacheManager<TransactionState, BcnTransaction> _transactionStateCacheManager;
        private readonly IDiffService<BcnTransaction> _diffService;

        public TransactionsScanService(
            IPayInternalClient payInternalClient,
            QBitNinjaClient qBitNinjaClient,
            ITransactionStateCacheManager<TransactionState, BcnTransaction> transactionStateCacheManager,
            IDiffService<BcnTransaction> diffService,
            ILog log) : base(
            payInternalClient,
            qBitNinjaClient,
            log)
        {
            _transactionStateCacheManager = transactionStateCacheManager ?? throw new ArgumentNullException(nameof(transactionStateCacheManager));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
        }

        public override async Task ExecuteAsync()
        {
            //todo: move cleaning out of this service
            await _transactionStateCacheManager.ClearOutOfDateAsync();

            IEnumerable<BcnTransaction> initialTransactions = (await _transactionStateCacheManager.GetStateAsync()).Select(x => x.Transaction).ToList();

            var currentTransactions = (await GetBcnTransactionsAsync(initialTransactions.Select(x => x.Id))).ToList();

            var updatedTransactions = _diffService.Diff(initialTransactions, currentTransactions);

            await BroadcastAsync(updatedTransactions);

            await _transactionStateCacheManager.UpdateTransactionsAsync(currentTransactions);
        }

        public override Task CreateTransactionAsync(BcnTransaction tx)
        {
            throw new NotSupportedException();
        }

        public override async Task UpdateTransactionAsync(BcnTransaction tx)
        {
            //todo: FirstSeen has to be a part of domain model BcnTransaction, also a part of NewTransactionMessage
            var txDetails = await QBitNinjaClient.GetTransaction(new uint256(tx.Id));

            await PayInternalClient.UpdateTransactionAsync(new UpdateTransactionRequest
            {
                Amount = tx.Amount,
                Confirmations = tx.Confirmations,
                BlockId = tx.BlockId,
                TransactionId = tx.Id,
                FirstSeen = txDetails.FirstSeen.DateTime
            });
        }

        public override async Task<IEnumerable<BcnTransaction>> GetBcnTransactionsAsync(IEnumerable<string> transactionIds)
        {
            var transactions = new List<GetTransactionResponse>();

            foreach (var batch in transactionIds.Batch(BatchPieceSize))
            {
                await Task.WhenAll(batch.Select(transactionId => QBitNinjaClient
                    .GetTransaction(new uint256(transactionId))
                    .ContinueWith(t =>
                    {
                        lock (transactions)
                        {
                            transactions.Add(t.Result);
                        }
                    })));
            }

            return transactions.Select(tx => tx.ToDomain());
        }
    }
}
