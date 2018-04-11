using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using System;
using System.Collections.Generic;

namespace Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache
{
    public class WalletState
    {
        public string Address { get; set; }

        public DateTime DueDate { get; set; }

        public BlockchainType Blockchain { get; set; }

        public IEnumerable<PaymentBcnTransaction> Transactions { get; set; }
    }
}
