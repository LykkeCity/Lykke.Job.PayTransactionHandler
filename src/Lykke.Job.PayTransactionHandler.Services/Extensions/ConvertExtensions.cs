﻿using System;
using System.Linq;
using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using NBitcoin;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services.Extensions
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
                Blockchain = Enum.Parse<BlockchainType>(src.Blockchain.ToString()),
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
                Blockchain = Enum.Parse<BlockchainType>(src.Blockchain.ToString()),
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
                Amount = src.SpentCoins.Sum(x => x.TxOut.Value.ToDecimal(MoneyUnit.BTC)),
                Confirmations = src.Block?.Confirmations ?? 0,
                BlockId = src.Block?.BlockId?.ToString(),
                Blockchain = BlockchainType.Bitcoin,
                AssetId = nameof(MoneyUnit.BTC)
            };
        }

        public static CreateTransactionRequest ToCreateRequest(this PaymentBcnTransaction src, GetTransactionResponse txDetails, Network network)
        {
            return new CreateTransactionRequest
            {
                WalletAddress = src.WalletAddress,
                Amount = src.Amount,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                FirstSeen = txDetails.FirstSeen.DateTime,
                TransactionId = src.Id,
                Blockchain = Enum.Parse<Service.PayInternal.Client.Models.BlockchainType>(src.Blockchain.ToString()),
                AssetId = src.AssetId,
                SourceWalletAddresses = txDetails.GetSourceWalletAddresses(network).Select(x => x.ToString()).ToArray()
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
                Blockchain = Enum.Parse<Service.PayInternal.Client.Models.BlockchainType>(src.Blockchain.ToString()),
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
