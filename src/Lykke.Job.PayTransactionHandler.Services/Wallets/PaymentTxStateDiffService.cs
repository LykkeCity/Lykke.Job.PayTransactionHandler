using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    /// <summary>
    /// Returns payment transactions updated compared to initial transactions state or absent in initial state.
    /// Assumes, transactions can't be deleted from initial state but only added or updated
    /// </summary>
    public class PaymentTxStateDiffService : IDiffService<PaymentBcnTransaction>
    {
        private readonly int _confirmationsLimit;

        public PaymentTxStateDiffService(int confirmationsLimit)
        {
            _confirmationsLimit = confirmationsLimit;
        }

        public IEnumerable<DiffResult<PaymentBcnTransaction>> Diff(IEnumerable<PaymentBcnTransaction> initialState, IEnumerable<PaymentBcnTransaction> updatedState)
        {
            var result = new List<DiffResult<PaymentBcnTransaction>>();

            foreach (var tx in updatedState)
            {
                var initialTx = initialState.SingleOrDefault(x => x.Id == tx.Id && x.WalletAddress == tx.WalletAddress);

                if (initialTx?.Confirmations >= _confirmationsLimit)
                {
                    continue;
                }

                var isNew = initialTx == null;

                if (!initialState.Any(x => tx.Equals(x)))
                {
                    result.Add(new DiffResult<PaymentBcnTransaction>
                    {
                        CompareState = isNew ? DiffState.New : DiffState.Updated,
                        Object = tx
                    });
                }
            }

            return result;
        }

        public DiffResult<PaymentBcnTransaction> Diff(PaymentBcnTransaction initialState, PaymentBcnTransaction updatedState)
        {
            throw new NotImplementedException();
        }
    }
}
