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

                DiffResult<BcnTransaction> txDiff = DiffSingleTransaction(initialTx, tx);

                if (txDiff != null)
                {
                    result.Add(txDiff);
                }
            }

            return result;
        }

        public DiffResult<BcnTransaction> Diff(BcnTransaction initialTx, BcnTransaction updatedTx)
        {
            return DiffSingleTransaction(initialTx, updatedTx);
        }

        private DiffResult<BcnTransaction> DiffSingleTransaction(BcnTransaction initialTx, BcnTransaction updatedTx)
        {
            _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(DiffSingleTransaction),
                $"Diffing transaction {updatedTx.Id}. Confirmations: {updatedTx.Confirmations}");

            if (initialTx?.Confirmations >= _confirmationsLimit)
            {
                _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(DiffSingleTransaction),
                    $"Confirmations limit reached ({_confirmationsLimit}), will skip transaction");

                return null;
            }

            var isNew = initialTx == null;

            if (isNew)
            {
                _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(DiffSingleTransaction),
                    $"Found new transaction: {updatedTx.Id}");
            }
            else
            {
                _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(DiffSingleTransaction),
                    $"Not new transaction. Will try to compare");
            }

            if (!updatedTx.Equals(initialTx))
            {
                _log.WriteInfoAsync(nameof(TxStateDiffService), nameof(DiffSingleTransaction),
                    $"Transaction {updatedTx.Id} has changes.");

                return new DiffResult<BcnTransaction>
                {
                    CompareState = isNew ? DiffState.New : DiffState.Updated,
                    Object = updatedTx
                };
            }

            return null;
        }
    }
}
