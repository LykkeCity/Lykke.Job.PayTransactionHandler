namespace Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        public string EthereumConnectionString { get; set; }
        public string LykkePayConnectionString { get; set; }
        public string WalletsExchangeName { get; set; }
        public string TransactionsExchangeName { get; set; }
        public string EthereumEventsExchangeName { get; set; }
        public string EthereumEventsQueueName { get; set; }
    }
}
