using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using QBitNinja.Client.Models;

namespace Lykke.Job.PayTransactionHandler.Services.Extensions
{
    public static class BitcoinExtensions
    {
        public static BitcoinAddress GetDestinationAddress(this ICoin src, Network network)
        {
            return src?.TxOut?.ScriptPubKey?.GetDestinationAddress(network);
        }

        public static IEnumerable<BitcoinAddress> GetSourceWalletAddresses(this GetTransactionResponse src, Network network)
        {
            return src?.SpentCoins?.Select(x => x.GetDestinationAddress(network)).Where(x => x != null);
        }
    }
}
