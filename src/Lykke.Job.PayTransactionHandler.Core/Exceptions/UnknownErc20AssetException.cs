using System;
using System.Runtime.Serialization;

namespace Lykke.Job.PayTransactionHandler.Core.Exceptions
{
    public class UnknownErc20AssetException : Exception
    {
        public UnknownErc20AssetException()
        {
        }

        public UnknownErc20AssetException(string asset) : base("Unknown erc20 asset")
        {
            Asset = asset;
        }

        public UnknownErc20AssetException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownErc20AssetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Asset { get; set; }
    }
}
