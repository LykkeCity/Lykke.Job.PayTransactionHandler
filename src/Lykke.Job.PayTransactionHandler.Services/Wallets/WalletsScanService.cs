using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
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
            ILog log,
            string bitcoinNetwork)
        {
            _cacheMaintainer = cacheMaintainer ?? throw new ArgumentNullException(nameof(cacheMaintainer));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _bitcoinNetwork = Network.GetNetwork(bitcoinNetwork);
            _log = log?.CreateComponentScope(nameof(WalletsScanService)) ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task ExecuteAsync()
        {
            IEnumerable<WalletState> cacheState = await _cacheMaintainer.GetAsync();

            foreach (var walletState in cacheState)
            {
                if (walletState.Blockchain == BlockchainType.Ethereum)
                    continue;

                BalanceModel balance;

                try
                {
                    balance = await _qBitNinjaClient.GetBalance(BitcoinAddress.Create(walletState.Address));

                    //todo: remove logging
                    await _log.WriteInfoAsync(nameof(ExecuteAsync), new
                    {
                        walletState,
                        ninjaOperations = balance?.Operations?
                            .Where(o => o.ReceivedCoins.Any(coin =>
                                coin.GetDestinationAddress(_bitcoinNetwork).ToString().Equals(walletState.Address)))
                            .Select(x => x.ToDomainPaymentTransaction(walletState.Address))
                    }.ToJson(), "Getting balance for wallet");

                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync("Getting balance from ninja", walletState?.ToJson(), ex);

                    continue;
                }

                IEnumerable<PaymentBcnTransaction> bcnTransactions = balance?.Operations?
                    .Where(o => o.ReceivedCoins.Any(coin => coin.GetDestinationAddress(_bitcoinNetwork).ToString().Equals(walletState.Address)))
                    .Select(x => x.ToDomainPaymentTransaction(walletState.Address)).ToList();

                IEnumerable<PaymentBcnTransaction> cacheTransactions = walletState.Transactions;

                IEnumerable<DiffResult<PaymentBcnTransaction>> diff = _diffService.Diff(cacheTransactions, bcnTransactions);

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

                                await _payInternalClient.CreatePaymentTransactionAsync(createRequest);
                            }
                            catch (Exception ex)
                            {
                                await _log.WriteErrorAsync(nameof(ExecuteAsync), createRequest?.ToJson(), ex);

                                continue;
                            }

                            break;

                        case DiffState.Updated:

                            UpdateTransactionRequest updateRequest = null;

                            try
                            {
                                updateRequest = tx.ToUpdateRequest();

                                await _payInternalClient.UpdateTransactionAsync(updateRequest);
                            }
                            catch (Exception ex)
                            {
                                await _log.WriteErrorAsync(nameof(ExecuteAsync), updateRequest?.ToJson(), ex);

                                continue;
                            }

                            break;

                        default: throw new Exception("Unknown transactions diff state");
                    }
                }

                walletState.Transactions = bcnTransactions;

                await _cacheMaintainer.SetItemAsync(walletState);
            }
        }
    }
}
