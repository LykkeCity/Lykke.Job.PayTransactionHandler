using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using NBitcoin;
using QBitNinja.Client.Models;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Job.PayTransactionHandler.Services.CommonModels
{
    class CommonBalanceModel
    {
        public string WalletAddress { get; set; }
        public BalanceModel Balance { get; set; }

        public IEnumerable<BlockchainTransaction> GetTransactions()
        {
            return Balance?.Operations?.Select(x => new BlockchainTransaction
            {
                WalletAddress = WalletAddress,
                Amount = (double)x.Amount.ToDecimal(MoneyUnit.Satoshi),
                AssetId = nameof(MoneyUnit.Satoshi),
                Blockchain = "Bitcoin",
                Id = x.TransactionId.ToString(),
                BlockId = x.BlockId?.ToString(),
                Confirmations = x.Confirmations
            });
        }
    }
}
