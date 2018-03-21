﻿using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using NBitcoin;
using QBitNinja.Client.Models;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.PayTransactionHandler.Core;

namespace Lykke.Job.PayTransactionHandler.Services.CommonModels
{
    internal class CommonBalanceModel
    {
        public string WalletAddress { get; set; }
        public BalanceModel Balance { get; set; }

        public IEnumerable<PaymentBcnTransaction> GetPaymentTransactions(Network network)
        {
            return Balance?.Operations?
                .Where(o => o.ReceivedCoins.Any(coin =>
                    coin.GetDestinationAddress(network).ToString().Equals(WalletAddress))).Select(x =>
                    new PaymentBcnTransaction
                    {
                        WalletAddress = WalletAddress,
                        Amount = x.Amount.ToDecimal(MoneyUnit.Satoshi),
                        AssetId = nameof(MoneyUnit.Satoshi),
                        Blockchain = BlockchainType.Bitcoin.ToString(),
                        Id = x.TransactionId.ToString(),
                        BlockId = x.BlockId?.ToString(),
                        Confirmations = x.Confirmations
                    });
        }
    }
}
