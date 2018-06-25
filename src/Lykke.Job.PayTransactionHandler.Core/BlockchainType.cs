namespace Lykke.Job.PayTransactionHandler.Core
{
    /// <summary>
    /// Blockchain type
    /// </summary>
    public enum BlockchainType
    {
        /// <summary>
        /// Not a blockchain
        /// </summary>
        None = 0,

        /// <summary>
        /// Bitcoin blockchain
        /// </summary>
        Bitcoin,

        /// <summary>
        /// Ethereum blockchain
        /// </summary>
        Ethereum,

        /// <summary>
        /// Ethereum blockchain with IATA specific implementation
        /// </summary>
        EthereumIata
    }
}
