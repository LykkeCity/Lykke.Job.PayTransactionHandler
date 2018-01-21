using System;
using System.Collections.Generic;
using System.Linq;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services
{
    /// <summary>
    /// Returns transactions updated compared to initial transactions state or absent in initial state.
    /// Assumes, transactions can't be deleted from initial state but only added or updated
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TransactionStateDiffService : IDiffService<BlockchainTransaction>
    {
        private readonly int _confirmationsLimit;
        private readonly ILog _log;

        public TransactionStateDiffService(int confirmationsLimit, ILog log)
        {
            _confirmationsLimit = confirmationsLimit;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IEnumerable<DiffResult<BlockchainTransaction>> Diff(IEnumerable<BlockchainTransaction> initialState, IEnumerable<BlockchainTransaction> updatedState)
        {
            var result = new List<DiffResult<BlockchainTransaction>>();

            foreach (var tx in updatedState)
            {
                var initialTx = initialState.SingleOrDefault(x => x.Id == tx.Id);

                if (initialTx?.Confirmations >= _confirmationsLimit)
                    continue;

                var isNew = initialTx == null;

                if (!initialState.Any(x => tx.Equals(x)))
                {
                    result.Add(new DiffResult<BlockchainTransaction>
                    {
                        CompareState = isNew ? DiffState.New : DiffState.Updated,
                        Object = tx
                    });
                }
            }

            return result;
        }
    }
}
