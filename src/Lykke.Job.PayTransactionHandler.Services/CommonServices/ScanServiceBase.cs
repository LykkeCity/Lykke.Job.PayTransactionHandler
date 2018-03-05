using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services.CommonModels;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models;
using MoreLinq;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Services.CommonServices
{
    public abstract class ScanServiceBase : IScanService
    {
        protected readonly IPayInternalClient _payInternalClient;
        protected readonly QBitNinjaClient _qBitNinjaClient;
        protected readonly ILog _log;

        protected const int BatchPieceSize = 15;

        public ScanServiceBase(
            IPayInternalClient payInternalClient,
            QBitNinjaClient qBitNinjaClient,
            ILog log)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _qBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public abstract Task Execute();

        protected virtual async Task Broadcast(IEnumerable<DiffResult<BlockchainTransaction>> updated)
        {
            foreach (var diffResult in updated)
            {
                var tx = diffResult.Object;

                switch (diffResult.CompareState)
                {
                    case DiffState.New:

                        var txDetails = await _qBitNinjaClient.GetTransaction(new uint256(tx.Id));

                        await CreateTransaction(tx, txDetails);

                        break;

                    case DiffState.Updated:

                        await UpdateTransaction(tx);

                        break;

                    default: throw new Exception("Unknown transactions diff state");
                }
            }
        }

        protected virtual async Task CreateTransaction(BlockchainTransaction t, GetTransactionResponse tDetails)
        {
            await _payInternalClient.CreatePaymentTransaction(new CreateTransactionRequest
            {
                WalletAddress = t.WalletAddress,
                Amount = t.Amount,
                FirstSeen = tDetails.FirstSeen.DateTime,
                TransactionId = t.Id,
                Confirmations = t.Confirmations,
                BlockId = t.BlockId,
                Blockchain = t.Blockchain,
                AssetId = t.AssetId
            });
        }

        protected virtual async Task UpdateTransaction(BlockchainTransaction t)
        {
            await _payInternalClient.UpdateTransaction(new UpdateTransactionRequest
            {
                WalletAddress = t.WalletAddress,
                Amount = t.Amount,
                Confirmations = t.Confirmations,
                BlockId = t.BlockId,
                TransactionId = t.Id
            });
        }

        protected async Task<IEnumerable<BlockchainTransaction>> GetTransactions(IEnumerable<string> addresses)
        {
            var balances = new List<CommonBalanceModel>();

            foreach (var batch in addresses.Batch(BatchPieceSize))
            {
                //todo: use continuation token to get full list of operations
                await Task.WhenAll(batch.Select(address => _qBitNinjaClient.GetBalance(BitcoinAddress.Create(address))
                    .ContinueWith(t =>
                    {
                        lock (balances)
                        {
                            balances.Add(new CommonBalanceModel
                            {
                                WalletAddress = address,
                                Balance = t.Result
                            });
                        }
                    })));
            }

            return balances.SelectMany(x => x.GetTransactions());
        }
    }
}
