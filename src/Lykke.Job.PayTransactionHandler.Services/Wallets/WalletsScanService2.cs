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

        public WalletsScanService2(
            ICacheMaintainer<WalletState> cacheMaintainer,
            QBitNinjaClient qBitNinjaClient,
            IDiffService<PaymentBcnTransaction> diffService,
            IPayInternalClient payInternalClient)
        {
            _cacheMaintainer = cacheMaintainer ?? throw new ArgumentNullException(nameof(cacheMaintainer));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
        }

        public async Task Execute()
        {
            IEnumerable<WalletState> cacheState = await _cacheMaintainer.Get();

            foreach (var walletState in cacheState)
            {
                BalanceModel balance = await _qBitNinjaClient.GetBalance(BitcoinAddress.Create(walletState.Address));

                IEnumerable<PaymentBcnTransaction> bcnTransactions = balance.Operations
                    ?.Select(x => x.ToDomainPaymentTransaction(walletState.Address)).ToList();

                IEnumerable<PaymentBcnTransaction> cacheTransactions = walletState.Transactions;

                IEnumerable<DiffResult<PaymentBcnTransaction>> diff = _diffService.Diff(cacheTransactions, bcnTransactions);

                foreach (var diffResult in diff)
                {
                    var tx = diffResult.Object;

                    switch (diffResult.CompareState)
                    {
                        case DiffState.New:

                            var txDetails = await _qBitNinjaClient.GetTransaction(new uint256(tx.Id));

                            await _payInternalClient.CreatePaymentTransaction(
                                tx.ToCreateRequest(txDetails.FirstSeen.DateTime));

                            break;

                        case DiffState.Updated:

                            await _payInternalClient.UpdateTransaction(tx.ToUpdateRequest());

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
