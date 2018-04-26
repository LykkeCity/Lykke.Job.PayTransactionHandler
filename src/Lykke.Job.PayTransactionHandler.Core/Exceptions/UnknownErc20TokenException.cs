using System;
using System.Runtime.Serialization;

namespace Lykke.Job.PayTransactionHandler.Core.Exceptions
{
    public class UnknownErc20TokenException : Exception
    {
        public UnknownErc20TokenException()
        {
        }

        public UnknownErc20TokenException(string tokenAddress) : base("Unknown erc20 token")
        {
            TokenAddress = tokenAddress;
        }

        public UnknownErc20TokenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownErc20TokenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string TokenAddress { get; set; }
    }
}
