using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using NBitcoin;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services.Extensions
{
    public static class NinjaExtensions
    {
        public static PaymentBcnTransaction ToDomainPaymentTransaction(this BalanceOperation src, string walletAddress,
            BlockchainType bcnType = BlockchainType.Bitcoin)
        {
            return new PaymentBcnTransaction
            {
                WalletAddress = walletAddress,
                Amount = src.Amount.ToDecimal(MoneyUnit.BTC),
                AssetId = nameof(MoneyUnit.BTC),
                Blockchain = bcnType,
                Id = src.TransactionId.ToString(),
                BlockId = src.BlockId?.ToString(),
                Confirmations = src.Confirmations
            };
        }
    }
}
