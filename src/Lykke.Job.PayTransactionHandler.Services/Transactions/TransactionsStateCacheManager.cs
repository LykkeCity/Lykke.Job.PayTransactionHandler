using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Extensions;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Wallets;

namespace Lykke.Job.PayTransactionHandler.Services.Transactions
{
    public class TransactionsStateCacheManager : StateCacheManagerBase<TransactionState, BcnTransaction>
    {
        private readonly int _confirmationsLimit;

        public TransactionsStateCacheManager(
            ICache<TransactionState> payTransactionCache,
            IPayInternalClient payInternalClient,
            int confirmationsLimit,
            ILog log) : base(
            payTransactionCache,
            payInternalClient,
            log)
        {
            _confirmationsLimit = confirmationsLimit;
        }

        public override async Task ClearOutOfDate()
        {
            IEnumerable<TransactionState> state = await Cache.Get();

            IEnumerable<TransactionState> toRemove =
                state.Where(x => x.IsConfirmed(_confirmationsLimit) || x.IsExpired());

            foreach (TransactionState txState in toRemove)
            {
                if (txState.IsExpired())
                {
                    //todo: notify PayInternal about transaction expired by DueDate
                }

                await Cache.Remove(txState);

                await Log.WriteInfoAsync(nameof(TransactionsStateCacheManager), nameof(ClearOutOfDate),
                    $"Cleared transaction {txState.Transaction.Id} from cache with confirmations = {txState.Transaction.Confirmations}");
            }
        }

        public override async Task UpdateTransactions(IEnumerable<BcnTransaction> transactions)
        {
            IEnumerable<TransactionState> state = (await Cache.Get()).ToList();

            foreach (var blockchainTransaction in transactions)
            {
                TransactionState transactionState = state.Single(x => blockchainTransaction.Id == x.Transaction.Id);

                transactionState.Transaction = blockchainTransaction;

                await Cache.Update(transactionState);

                await Log.WriteInfoAsync(nameof(TransactionsStateCacheManager), nameof(UpdateTransactions),
                    $"Updated transaction {transactionState.Transaction.Id} in cache");
            }
        }

        public override async Task Warmup()
        {
            IEnumerable<TransactionStateResponse> transactions = await PayInternalClient.GetAllMonitoredTransactions();

            await Cache.AddRange(transactions.Select(x => new TransactionState
            {
                Transaction = x.ToDomainTransaction(),
                DueDate = x.DueDate
            }));
        }
    }
}
