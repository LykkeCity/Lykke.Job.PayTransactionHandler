using System;
using System.Linq;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
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
                Amount = (decimal) src.Amount,
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
                Amount = (decimal) src.Amount,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                Blockchain = src.Blockchain,
                AssetId = src.AssetId
            };
        }

        public static TransactionState ToDomainState(this TransactionStateResponse src)
        {
            return new TransactionState
            {
                DueDate = src.DueDate,
                Transaction = src.ToDomainTransaction()
            };
        }

        public static BcnTransaction ToDomain(this GetTransactionResponse src)
        {
            return new BcnTransaction
            {
                Id = src.TransactionId.ToString(),
                Amount = src.SpentCoins.Sum(x => x.TxOut.Value.ToDecimal(MoneyUnit.Satoshi)),
                Confirmations = src.Block?.Confirmations ?? 0,
                BlockId = src.Block?.BlockId?.ToString(),
                Blockchain = "Bitcoin",
                AssetId = nameof(MoneyUnit.Satoshi)
            };
        }

        public static CreateTransactionRequest ToCreateRequest(this PaymentBcnTransaction src, DateTime firstSeen)
        {
            return new CreateTransactionRequest
            {
                WalletAddress = src.WalletAddress,
                Amount = src.Amount,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                FirstSeen = firstSeen,
                TransactionId = src.Id,
                Blockchain = src.Blockchain,
                AssetId = src.AssetId
            };
        }

        public static UpdateTransactionRequest ToUpdateRequest(this PaymentBcnTransaction src)
        {
            return new UpdateTransactionRequest
            {
                WalletAddress = src.WalletAddress,
                Amount = src.Amount,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                TransactionId = src.Id
            };
        }

        public static CreateTransactionRequest ToCreateRequest(this BcnTransaction src, DateTime firstSeen)
        {
            return new CreateTransactionRequest
            {
                Amount = src.Amount,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                FirstSeen = firstSeen,
                AssetId = src.AssetId,
                Blockchain = src.Blockchain,
                TransactionId = src.Id
            };
        }

        public static UpdateTransactionRequest ToUpdateRequest(this BcnTransaction src, DateTime firstSeen)
        {
            return new UpdateTransactionRequest
            {
                BlockId = src.BlockId,
                Confirmations = src.Confirmations,
                FirstSeen = firstSeen,
                TransactionId = src.Id,
                Amount = src.Amount
            };
        }
    }
}
