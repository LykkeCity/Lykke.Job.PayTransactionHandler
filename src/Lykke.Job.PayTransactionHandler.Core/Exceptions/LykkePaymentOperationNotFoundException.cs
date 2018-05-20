using System;
using System.Runtime.Serialization;

namespace Lykke.Job.PayTransactionHandler.Core.Exceptions
{
    public class LykkePaymentOperationNotFoundException : Exception
    {
        public LykkePaymentOperationNotFoundException()
        {
        }

        public LykkePaymentOperationNotFoundException(Guid operationId) : base("Lykke payment operation not found")
        {
            OperationId = operationId;
        }

        public LykkePaymentOperationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LykkePaymentOperationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Guid OperationId { get; set; }
    }
}
