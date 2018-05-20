using System;

namespace Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings
{
    public class PayTransactionHandlerSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
        public TimeSpan WalletsScanPeriod { get; set; }
        public TimeSpan TransactionsScanPeriod { get; set; }
        public TimeSpan CacheExpirationHandlingPeriod { get; set; }
        public BlockchainSettings Blockchain { get; set; }
        public BitcoinSettings Bitcoin { get; set; }
        public CqrsSettings Cqrs { get; set; }
    }

    public class BlockchainSettings
    {
        public int ConfirmationsToSucceed { get; set; }
    }

    public class BitcoinSettings
    {
        public string Network { get; set; }
    }

    public class CqrsSettings
    {
        public int RetryDelayInMilliseconds { get; set; }
    }
}
