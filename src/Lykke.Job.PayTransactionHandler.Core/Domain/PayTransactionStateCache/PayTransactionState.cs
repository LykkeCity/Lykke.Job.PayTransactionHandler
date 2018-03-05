using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.PayTransactionHandler.Core.Domain.PayTransactionStateCache
{
    public class PayTransactionState
    {
        public string TransactionId { get; set; }
        public string WalletAddress { get; set; }
        public decimal Amount { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
    }
}
