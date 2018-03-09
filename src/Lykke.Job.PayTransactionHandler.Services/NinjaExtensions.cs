using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using NBitcoin;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public static class NinjaExtensions
    {
        public static PaymentBcnTransaction ToDomainPaymentTransaction(this BalanceOperation src, string walletAddress,
            BlockchainType bcnType = BlockchainType.Bitcoin)
        {
            return new PaymentBcnTransaction
            {
                WalletAddress = walletAddress,
                Amount = (double) src.Amount.ToDecimal(MoneyUnit.Satoshi),
                AssetId = nameof(MoneyUnit.Satoshi),
                Blockchain = bcnType.ToString(),
                Id = src.TransactionId.ToString(),
                BlockId = src.BlockId?.ToString(),
                Confirmations = src.Confirmations
            };
        }
    }
}
