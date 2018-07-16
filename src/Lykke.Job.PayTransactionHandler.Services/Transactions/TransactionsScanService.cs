using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.PayTransactionHandler.Core;
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
            ILogFactory logFactory)
        {
            _cacheMaintainer = cacheMaintainer ?? throw new ArgumentNullException(nameof(cacheMaintainer));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _log = logFactory.CreateLog(this);
        }

        public async Task ExecuteAsync()
        {
            IEnumerable<TransactionState> cacheState = await _cacheMaintainer.GetAsync();

            foreach (TransactionState cacheTxState in cacheState)
            {
                BcnTransaction cacheTx = cacheTxState.Transaction;

                if (cacheTx.Blockchain != BlockchainType.Bitcoin)
                    continue;

                GetTransactionResponse bcnTransactionState;

                try
                {
                    bcnTransactionState = await _qBitNinjaClient.GetTransaction(new uint256(cacheTx.Id));
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Getting transaction from ninja", cacheTxState);

                    continue;
                }

                // if transaction has not been indexed by ninja yet
                if (bcnTransactionState == null) continue;

                BcnTransaction bcnTx = bcnTransactionState.ToDomain();

                DiffResult<BcnTransaction> diffResult = _diffService.Diff(cacheTx, bcnTx);

                if (diffResult != null)
                {
                    var tx = diffResult.Object;

                    switch (diffResult.CompareState)
                    {
                        case DiffState.New:

                            _log.Warning("New transactions are not supported by watcher", context: tx);

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
                                _log.Error(ex, context: updateRequest);
                            }

                            break;

                        default:
                            throw new Exception("Unknown transactions diff state");
                    }

                    cacheTxState.Transaction = bcnTx;

                    try
                    {
                        await _cacheMaintainer.SetItemAsync(cacheTxState);
                    }
                    catch (Exception ex)
                    {

                        _log.Error(ex, "Updating transaction cache", cacheTxState);

                        continue;
                    }
                }
            }
        }
    }
}
