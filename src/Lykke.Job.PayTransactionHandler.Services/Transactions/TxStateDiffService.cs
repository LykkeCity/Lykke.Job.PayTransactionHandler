using System;
using System.Collections.Generic;
using System.Linq;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services.Transactions
{
    /// <summary>
    /// Returns transactions updated compared to initial transactions state or absent in initial state.
    /// Assumes, transactions can't be deleted from initial state but only added or updated
    /// </summary>
    public class TxStateDiffService : IDiffService<BcnTransaction>
    {
        private readonly int _confirmationsLimit;
        private readonly ILog _log;

        public TxStateDiffService(int confirmationsLimit, ILog log)
        {
            _confirmationsLimit = confirmationsLimit;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IEnumerable<DiffResult<BcnTransaction>> Diff(IEnumerable<BcnTransaction> initialState, IEnumerable<BcnTransaction> updatedState)
        {
            var result = new List<DiffResult<BcnTransaction>>();

            foreach (var tx in updatedState)
            {
                var initialTx = initialState.SingleOrDefault(x => x.Id == tx.Id);

                _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(Diff),
                    $"Diffing transaction {tx.Id}. Confirmations: {tx.Confirmations}");

                if (initialTx?.Confirmations >= _confirmationsLimit)
                {
                    _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(Diff),
                        $"Confirmations limit reached ({_confirmationsLimit}), will skip transaction");

                    continue;
                }

                var isNew = initialTx == null;

                if (isNew)
                {
                    _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(Diff),
                        $"Found new transaction: {tx.Id}");
                }
                else
                {
                    _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(Diff),
                        $"Not new transaction. Will try to compare");
                }

                if (!initialState.Any(x => tx.Equals(x)))
                {
                    _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(Diff),
                        $"Transaction {tx.Id} has changes.");

                    result.Add(new DiffResult<BcnTransaction>
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
