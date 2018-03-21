using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services.Transactions
{
    public class TransactionsScanService2 : IScanService
    {
        private readonly ICacheMaintainer<TransactionState> _cacheMaintainer;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly IDiffService<BcnTransaction> _diffService;
        private readonly IPayInternalClient _payInternalClient;

        public TransactionsScanService2(
            ICacheMaintainer<TransactionState> cacheMaintainer,
            QBitNinjaClient qBitNinjaClient,
            IDiffService<BcnTransaction> diffService,
            IPayInternalClient payInternalClient)
        {
            _cacheMaintainer = cacheMaintainer ?? throw new ArgumentNullException(nameof(cacheMaintainer));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
        }

        public async Task ExecuteAsync()
        {
            IEnumerable<TransactionState> cacheState = await _cacheMaintainer.GetAsync();

            foreach (TransactionState cacheTxState in cacheState)
            {
                BcnTransaction cacheTx = cacheTxState.Transaction;

                GetTransactionResponse bcnTransactionState =
                    await _qBitNinjaClient.GetTransaction(new uint256(cacheTx.Id));

                BcnTransaction bcnTx = bcnTransactionState.ToDomain();

                DiffResult<BcnTransaction> diffResult = _diffService.Diff(cacheTx, bcnTx);

                if (diffResult != null)
                {
                    var tx = diffResult.Object;

                    switch (diffResult.CompareState)
                    {
                        case DiffState.New:
                            throw new NotSupportedException();

                        case DiffState.Updated:

                            await _payInternalClient.UpdateTransactionAsync(
                                tx.ToUpdateRequest(bcnTransactionState.FirstSeen.DateTime));

                            break;

                        default:
                            throw new Exception("Unknown transactions diff state");
                    }

                    cacheTxState.Transaction = bcnTx;

                    await _cacheMaintainer.UpdateItemAsync(cacheTxState);
                }
            }
        }
    }
}
