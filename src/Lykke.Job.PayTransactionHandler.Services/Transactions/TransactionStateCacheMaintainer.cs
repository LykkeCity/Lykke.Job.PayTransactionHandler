using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Extensions;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using Lykke.Job.PayTransactionHandler.Services.Extensions;
using Lykke.Service.PayInternal.Client.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Job.PayTransactionHandler.Services.Transactions
{
    public class TransactionStateCacheMaintainer : ICacheMaintainer<TransactionState>
    {
        //todo: replace with IDistributedCache 
        private readonly IMemoryCache _cache;
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;
        private readonly int _confirmationsLimit;

        private const string CachePartitionName = "TransactionsState";

        public TransactionStateCacheMaintainer(
            [NotNull] IMemoryCache cache,
            [NotNull] IPayInternalClient payInternalClient,
            int confirmationsLimit,
            [NotNull] ILogFactory logFactory)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _confirmationsLimit = confirmationsLimit;
            _log = logFactory.CreateLog(this);
        }

        public async Task WarmUpAsync()
        {
            IEnumerable<TransactionStateResponse> transactionsState =
                await _payInternalClient.GetAllMonitoredTransactionsAsync();

            foreach (var txStateResponse in transactionsState)
            {
                TransactionState txState = txStateResponse.ToDomainState();

                await _cache.SetWithPartitionAsync(CachePartitionName, txState.Transaction.Id, txState);
            }
        }

        public async Task WipeAsync()
        {
            IEnumerable<TransactionState> cached = await _cache.GetPartitionAsync<TransactionState>(CachePartitionName);

            IEnumerable<TransactionState> toRemove =
                cached.Where(x => x.IsConfirmed(_confirmationsLimit) || x.IsExpired());

            foreach (TransactionState txState in toRemove)
            {
                if (txState.IsExpired())
                {
                    try
                    {
                        await _log.LogPayInternalExceptionIfAny(() => _payInternalClient.SetTransactionExpiredAsync(
                            new TransactionExpiredRequest
                            {
                                Blockchain = Enum.Parse<BlockchainType>(txState.Transaction.Blockchain.ToString()),
                                IdentityType = TransactionIdentityType.Hash,
                                Identity = txState.Transaction.Id
                            }));
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    _log.Info($"Cleared transaction {txState.Transaction.Id} from cache as expired");
                }

                await _cache.RemoveWithPartitionAsync(CachePartitionName, txState.Transaction.Id);

                _log.Info($"Cleared transactions {txState.Transaction.Id} from cache with duedate = {txState.DueDate}");
            }
        }

        public async Task SetItemAsync(TransactionState item)
        {
            await _cache.SetWithPartitionAsync(CachePartitionName, item.Transaction.Id, item);
        }

        public async Task<IEnumerable<TransactionState>> GetAsync()
        {
            return await _cache.GetPartitionAsync<TransactionState>(CachePartitionName);
        }
    }
}
