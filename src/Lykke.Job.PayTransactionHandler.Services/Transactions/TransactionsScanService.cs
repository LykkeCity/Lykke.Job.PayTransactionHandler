using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.Extensions;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services.Transactions
{
    public class TransactionsScanService : IScanService
    {
        private readonly ICacheMaintainer<TransactionState> _cacheMaintainer;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly IDiffService<BcnTransaction> _diffService;
        private readonly IPayInternalClient _payInternalClient;
        private readonly ILog _log;

        public TransactionsScanService(
            ICacheMaintainer<TransactionState> cacheMaintainer,
            QBitNinjaClient qBitNinjaClient,
            IDiffService<BcnTransaction> diffService,
            IPayInternalClient payInternalClient,
            ILog log)
        {
            _cacheMaintainer = cacheMaintainer ?? throw new ArgumentNullException(nameof(cacheMaintainer));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _log = log ?? throw new ArgumentNullException(nameof(log));
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

                            await _log.WriteWarningAsync(nameof(TransactionsScanService), nameof(ExecuteAsync),
                                tx?.ToJson(), "New transactions are not supported by watcher");

                            break;

                        case DiffState.Updated:

                            UpdateTransactionRequest updateRequest = null;

                            try
                            {
                                updateRequest = tx.ToUpdateRequest(bcnTransactionState.FirstSeen.DateTime);

                                await _payInternalClient.UpdateTransactionAsync(updateRequest);
                            }
                            catch (Exception ex)
                            {
                                await _log.WriteErrorAsync(nameof(TransactionsScanService), nameof(ExecuteAsync),
                                    updateRequest?.ToJson(), ex);
                            }

                            break;

                        default:
                            throw new Exception("Unknown transactions diff state");
                    }

                    cacheTxState.Transaction = bcnTx;

                    await _cacheMaintainer.SetItemAsync(cacheTxState);
                }
            }
        }
    }
}
