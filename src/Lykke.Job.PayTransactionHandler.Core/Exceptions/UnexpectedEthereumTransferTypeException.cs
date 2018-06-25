using System;
using System.Runtime.Serialization;
using Lykke.Job.EthereumCore.Contracts.Enums.LykkePay;

namespace Lykke.Job.PayTransactionHandler.Core.Exceptions
{
    public class UnexpectedEthereumTransferTypeException : Exception
    {
        public UnexpectedEthereumTransferTypeException()
        {
        }

        public UnexpectedEthereumTransferTypeException(SenderType transferType) : base("Unexpected ethereum transfer type")
        {
            TransferType = transferType;
        }

        public UnexpectedEthereumTransferTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnexpectedEthereumTransferTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SenderType TransferType { get; set; }
    }
}
