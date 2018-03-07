using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonServices;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models;

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

            foreach (var payTransactionState in state.Where(x => x.Transaction.Confirmations >= _confirmationsLimit))
            {
                await Cache.Remove(payTransactionState);

                await Log.WriteInfoAsync(nameof(TransactionsStateCacheManager), nameof(ClearOutOfDate),
                    $"Cleared transaction {payTransactionState.Transaction.Id} from cache with confirmations = {payTransactionState.Transaction.Confirmations}");
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
            // todo: call PayInternal to get not expired transactions
            var transactions = Enumerable.Empty<TransactionStateResponse>().ToList();

            await Cache.AddRange(transactions.Select(x => new TransactionState {Transaction = x.ToDomainTransaction()}));
        }
    }
}
