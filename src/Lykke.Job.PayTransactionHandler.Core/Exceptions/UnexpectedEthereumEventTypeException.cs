using System;
using System.Runtime.Serialization;
using Lykke.Job.EthereumCore.Contracts.Enums.LykkePay;

namespace Lykke.Job.PayTransactionHandler.Core.Exceptions
{
    public class UnexpectedEthereumEventTypeException : Exception
    {
        public UnexpectedEthereumEventTypeException()
        {
        }

        public UnexpectedEthereumEventTypeException(EventType eventType) : base("Unexpected ethereum event type")
        {
            EventType = eventType;
        }

        public UnexpectedEthereumEventTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnexpectedEthereumEventTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public EventType EventType { get; set; }
    }
}
