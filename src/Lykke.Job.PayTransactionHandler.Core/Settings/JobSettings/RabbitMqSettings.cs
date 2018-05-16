﻿namespace Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        public string ConnectionString { get; set; }
        public string WalletsExchangeName { get; set; }
        public string TransactionsExchangeName { get; set; }
        public string EthereumEventsExchangeName { get; set; }
    }
}
