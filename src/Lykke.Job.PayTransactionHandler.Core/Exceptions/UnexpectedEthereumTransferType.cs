using System;
using System.Runtime.Serialization;
using Lykke.Job.EthereumCore.Contracts.Enums.LykkePay;

namespace Lykke.Job.PayTransactionHandler.Core.Exceptions
{
    public class UnexpectedEthereumTransferType : Exception
    {
        public UnexpectedEthereumTransferType()
        {
        }

        public UnexpectedEthereumTransferType(SenderType transferType) : base("Unexpected ethereum transfer event type")
        {
            TransferType = transferType;
        }

        public UnexpectedEthereumTransferType(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnexpectedEthereumTransferType(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SenderType TransferType { get; set; }
    }
}
