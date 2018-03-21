using System;
using System.Collections.Generic;
using System.Linq;
using Common.Log;
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
        private readonly ILog _log;

        public PaymentTxStateDiffService(int confirmationsLimit, ILog log)
        {
            _confirmationsLimit = confirmationsLimit;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IEnumerable<DiffResult<PaymentBcnTransaction>> Diff(IEnumerable<PaymentBcnTransaction> initialState, IEnumerable<PaymentBcnTransaction> updatedState)
        {
            var result = new List<DiffResult<PaymentBcnTransaction>>();

            foreach (var tx in updatedState)
            {
                var initialTx = initialState.SingleOrDefault(x => x.Id == tx.Id && x.WalletAddress == tx.WalletAddress);

                _log.WriteInfoAsync(nameof(PaymentTxStateDiffService), nameof(Diff),
                    $"Diffing transaction {tx.Id}. Wallet {tx.WalletAddress}. Confirmations: {tx.Confirmations}");

                if (initialTx?.Confirmations >= _confirmationsLimit)
                {
                    _log.WriteInfoAsync(nameof(PaymentTxStateDiffService), nameof(Diff),
                        $"Confirmations limit reached ({_confirmationsLimit}), will skip transaction");

                    continue;
                }

                var isNew = initialTx == null;

                if (isNew)
                {
                    _log.WriteInfoAsync(nameof(PaymentTxStateDiffService), nameof(Diff),
                        $"Found new transaction: {tx.Id}");
                }
                else
                {
                    _log.WriteInfoAsync(nameof(PaymentTxStateDiffService), nameof(Diff),
                        $"Not new transaction. Will try to compare");
                }

                if (!initialState.Any(x => tx.Equals(x)))
                {
                    _log.WriteInfoAsync(nameof(PaymentTxStateDiffService), nameof(Diff),
                        $"Transaction {tx.Id} has changes.");

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
