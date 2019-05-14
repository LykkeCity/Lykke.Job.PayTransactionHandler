using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.Extensions;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    /// <summary>
    /// Scans bitcoin blockchain for only payment transactions (incoming payments)
    /// </summary>
    public class WalletsScanService : IScanService
    {
        private readonly ICacheMaintainer<WalletState> _cacheMaintainer;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly IDiffService<PaymentBcnTransaction> _diffService;
        private readonly IPayInternalClient _payInternalClient;
        private readonly Network _bitcoinNetwork;
        private readonly ILog _log;

        public WalletsScanService(
            ICacheMaintainer<WalletState> cacheMaintainer,
            QBitNinjaClient qBitNinjaClient,
            IDiffService<PaymentBcnTransaction> diffService,
            IPayInternalClient payInternalClient,
            ILogFactory logFactory,
            string bitcoinNetwork)
        {
            _cacheMaintainer = cacheMaintainer ?? throw new ArgumentNullException(nameof(cacheMaintainer));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _bitcoinNetwork = Network.GetNetwork(bitcoinNetwork);
            _log = logFactory.CreateLog(this);
        }

        public async Task ExecuteAsync()
        {
            IEnumerable<WalletState> cacheState = await _cacheMaintainer.GetAsync();

            foreach (var walletState in cacheState)
            {
                if (walletState.Blockchain != BlockchainType.Bitcoin)
                    continue;

                BalanceModel balance;

                try
                {
                    balance = await _qBitNinjaClient.GetBalance(BitcoinAddress.Create(walletState.Address));
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Getting balance from ninja", walletState.ToDetails());

                    continue;
                }

                IEnumerable<PaymentBcnTransaction> bcnTransactions =
                    GetIncomingPaymentOperations(balance, walletState.Address)?.ToList() ??
                    new List<PaymentBcnTransaction>();

                IEnumerable<PaymentBcnTransaction> cacheTransactions = walletState.Transactions;

                IEnumerable<DiffResult<PaymentBcnTransaction>> diff = _diffService.Diff(cacheTransactions, bcnTransactions);

                // catching case when ninja returns no transactions but should
                if (cacheTransactions.Any() && !bcnTransactions.Any())
                {
                    _log.Info("There are no transactions from ninja, but there are transactions in cache",
                        $"wallet: {walletState.Address}, walletState: {walletState.ToJson()}");
                }

                // catching case when there are any differences between ninja state and local cache state
                if (diff.Any())
                {
                    _log.Info("The difference between cache state and ninja detected",
                        $"wallet: {walletState.Address}, old txs: {walletState.Transactions.ToJson()}, new txs: {bcnTransactions.ToJson()}");
                }

                // approach to exit loop with switch statement inside
                bool isSyncError = false;
                
                foreach (var diffResult in diff)
                {
                    var tx = diffResult.Object;

                    switch (diffResult.CompareState)
                    {
                        case DiffState.New:

                            CreateTransactionRequest createRequest = null;

                            try
                            {
                                var txDetails = await _qBitNinjaClient.GetTransaction(new uint256(tx.Id));

                                createRequest = tx.ToCreateRequest(txDetails, _bitcoinNetwork);

                                _log.Info("New transaction detected",
                                    $"wallet: {walletState.Address}, Hash: {tx.Id}, FirstSeen: {txDetails.FirstSeen}");

                                await _log.LogPayInternalExceptionIfAny(() =>
                                    _payInternalClient.CreatePaymentTransactionAsync(createRequest));
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex, context: createRequest.ToDetails());

                                isSyncError = true;
                            }

                            break;

                        case DiffState.Updated:

                            UpdateTransactionRequest updateRequest = null;

                            try
                            {
                                updateRequest = tx.ToUpdateRequest();

                                _log.Info("Transaction update detected",
                                    $"wallet: {walletState.Address}, new tx state: {tx.ToJson()}");

                                await _log.LogPayInternalExceptionIfAny(() =>
                                    _payInternalClient.UpdateTransactionAsync(updateRequest));
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex, context: updateRequest.ToDetails());

                                isSyncError = true;
                            }

                            break;

                        default: throw new Exception("Unknown transactions diff state");
                    }
                    
                    // stop changes processing in case there is PayInternal sync error
                    if (isSyncError) break;
                }

                if (bcnTransactions.Any())
                {
                    walletState.Transactions = bcnTransactions;
                }

                // will sync internal cache with blockchain only if there were no errors while processing changes
                // otherwise the changes will be processed again in the next job cycle
                // WARNING: the approach will work only for single transactions update
                if (!isSyncError)
                {
                    try
                    {
                        await _cacheMaintainer.SetItemAsync(walletState);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Updating wallets cache", walletState.ToDetails());

                        continue;
                    }
                }
            }
        }

        private IEnumerable<PaymentBcnTransaction> GetIncomingPaymentOperations(BalanceModel balance, string walletAddress)
        {
            return balance?.Operations?
                .Where(o => o.ReceivedCoins.Any(coin =>
                                coin.GetDestinationAddress(_bitcoinNetwork).ToString().Equals(walletAddress)) &&
                            o.Amount.ToDecimal(MoneyUnit.BTC) > 0)
                .Select(x => x.ToDomainPaymentTransaction(walletAddress));
        }
    }
}
