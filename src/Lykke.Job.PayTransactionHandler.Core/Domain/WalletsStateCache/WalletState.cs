using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache
{
    public class WalletState
    {
        public string Address { get; set; }

        public DateTime DueDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainType Blockchain { get; set; }

        public IEnumerable<PaymentBcnTransaction> Transactions { get; set; }
    }
}
