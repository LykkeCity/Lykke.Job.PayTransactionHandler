using System;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;

namespace Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache
{
    public class TransactionState
    {
        public DateTime DueDate { get; set; }
        public BcnTransaction Transaction { get; set; }
    }
}
