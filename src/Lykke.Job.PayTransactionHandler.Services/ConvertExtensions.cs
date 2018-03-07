﻿using System.Linq;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Service.PayInternal.Client.Models;
using NBitcoin;
using QBitNinja.Client.Models;

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

        public static PaymentBcnTransaction ToDomain(this TransactionStateResponse src)
        {
            return new PaymentBcnTransaction
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

        public static BcnTransaction ToDomainTransaction(this TransactionStateResponse src)
        {
            return new BcnTransaction
            {
                Id = src.Id,
                Amount = src.Amount,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                Blockchain = src.Blockchain,
                AssetId = src.AssetId
            };
        }

        public static BcnTransaction ToDomain(this GetTransactionResponse src)
        {
            return new BcnTransaction
            {
                Id = src.TransactionId.ToString(),
                Amount = (double)src.SpentCoins.Sum(x => x.TxOut.Value.ToDecimal(MoneyUnit.Satoshi)),
                Confirmations = src.Block?.Confirmations ?? 0,
                BlockId = src.Block?.BlockId?.ToString(),
                Blockchain = "Bitcoin",
                AssetId = nameof(MoneyUnit.Satoshi)
            };
        }
    }
}
