﻿using System;
using System.Numerics;
using Lykke.Job.EthereumCore.Contracts.Enums.LykkePay;

namespace Lykke.Job.PayTransactionHandler.Core
{
    public static class EthereumExtensions
    {
        public static decimal ToAmount(this string amount, int multiplier, int accuracy)
        {
            if (accuracy > multiplier)
                throw new ArgumentException("accuracy > multiplier");

            multiplier -= accuracy;

            var val = BigInteger.Parse(amount);
            var res = (decimal) (val / BigInteger.Pow(10, multiplier));
            res /= (decimal) Math.Pow(10, accuracy);

            return res;
        }

        public static BlockchainType GetBlockchainType(this WorkflowType src)
        {
            return src == WorkflowType.Airlines ? BlockchainType.EthereumIata : BlockchainType.Ethereum;
        }
    }
}
