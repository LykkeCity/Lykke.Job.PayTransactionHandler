using System.Collections.Generic;
using System.Linq;
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

        public TxStateDiffService(int confirmationsLimit)
        {
            _confirmationsLimit = confirmationsLimit;
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
            if (initialTx?.Confirmations >= _confirmationsLimit)
            {
                return null;
            }

            var isNew = initialTx == null;

            if (!updatedTx.Equals(initialTx))
            {
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
