using System.Linq;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Service.PayInternal.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public static class ConvertExtensions
    {
        public static WalletState ToDomain(this WalletStateResponse src)
        {
            return new WalletState
            {
                Address = src.Address,
                DueDate = src.DueDate,
                Transactions = src.Transactions.Select(x => x.ToDomain())
            };
        }

        public static BlockchainTransaction ToDomain(this TransactionStateResponse src)
        {
            return new BlockchainTransaction
            {
                Id = src.Id,
                Amount = src.Amount,
                BlockId = src.BlockId,
                Confirmations = src.Confirmations,
                WalletAddress = src.WalletAddress,
                Blockchain = src.Blockchain,
                AssetId = src.AssetId
            };
        }
    }
}
