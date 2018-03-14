using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services.Wallets
{
    public class WalletsScanService2 : IScanService
    {
        private readonly ICacheMaintainer<WalletState> _cacheMaintainer;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly IDiffService<PaymentBcnTransaction> _diffService;
        private readonly IPayInternalClient _payInternalClient;
        private readonly Network _bitcoinNetwork;

        public WalletsScanService2(
            ICacheMaintainer<WalletState> cacheMaintainer,
            QBitNinjaClient qBitNinjaClient,
            IDiffService<PaymentBcnTransaction> diffService,
            IPayInternalClient payInternalClient,
            string bitcoinNetwork)
        {
            _cacheMaintainer = cacheMaintainer ?? throw new ArgumentNullException(nameof(cacheMaintainer));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _bitcoinNetwork = Network.GetNetwork(bitcoinNetwork);
        }

        public async Task Execute()
        {
            IEnumerable<WalletState> cacheState = await _cacheMaintainer.Get();

            foreach (var walletState in cacheState)
            {
                BalanceModel balance = await _qBitNinjaClient.GetBalance(BitcoinAddress.Create(walletState.Address));

                IEnumerable<PaymentBcnTransaction> bcnTransactions = balance?.Operations
                    ?.Where(o => o.ReceivedCoins.Any(coin =>
                        coin.GetDestinationAddress(_bitcoinNetwork).ToString().Equals(walletState.Address)))
                    .Select(x => x.ToDomainPaymentTransaction(walletState.Address)).ToList();

                IEnumerable<PaymentBcnTransaction> cacheTransactions = walletState.Transactions;

                IEnumerable<DiffResult<PaymentBcnTransaction>> diff = _diffService.Diff(cacheTransactions, bcnTransactions);

                foreach (var diffResult in diff)
                {
                    var tx = diffResult.Object;

                    switch (diffResult.CompareState)
                    {
                        case DiffState.New:

                            var txDetails = await _qBitNinjaClient.GetTransaction(new uint256(tx.Id));

                            await _payInternalClient.CreatePaymentTransactionAsync(tx.ToCreateRequest(txDetails, _bitcoinNetwork));

                            break;

                        case DiffState.Updated:

                            await _payInternalClient.UpdateTransactionAsync(tx.ToUpdateRequest());

                            break;

                        default: throw new Exception("Unknown transactions diff state");
                    }
                }

                walletState.Transactions = bcnTransactions;

                await _cacheMaintainer.UpdateItem(walletState);
            }
        }
    }
}
