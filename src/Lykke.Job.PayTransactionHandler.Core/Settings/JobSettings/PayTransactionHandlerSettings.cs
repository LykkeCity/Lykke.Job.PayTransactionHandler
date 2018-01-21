using System;

namespace Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings
{
    public class PayTransactionHandlerSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
        public TimeSpan WalletsScanPeriod { get; set; }
        public BlockchainSettings Blockchain { get; set; }
    }

    public class BlockchainSettings
    {
        public int ConfirmationsToSucceed { get; set; }
    }
}
