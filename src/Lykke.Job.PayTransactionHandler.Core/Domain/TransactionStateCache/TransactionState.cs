using Lykke.Job.PayTransactionHandler.Core.Domain.Common;

namespace Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache
{
    public class TransactionState
    {
        public BcnTransaction Transaction { get; set; }
    }
}
