namespace Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings
{
        public class PayTransactionHandlerSettings
        {
            public DbSettings Db { get; set; }
            public RabbitMqSettings Rabbit { get; set; }
        }
}
