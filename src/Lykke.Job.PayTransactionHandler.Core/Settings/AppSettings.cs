using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.Job.PayTransactionHandler.Core.Settings.SlackNotifications;
using Lykke.Service.PayInternal.Client;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.PayTransactionHandler.Core.Settings
{
    public class AppSettings
    {
        public PayTransactionHandlerSettings PayTransactionHandlerJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public PayInternalServiceClientSettings PayInternalServiceClient { get; set; }
        public NinjaServiceClientSettings NinjaServiceClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
        public TransportSettings Transports { get; set; }
        public OperationsServiceClientSettings OperationsServiceClient { get; set; }
    }

    public class NinjaServiceClientSettings
    {
        [HttpCheck("/")]
        public string ServiceUrl { get; set; }
    }

    public class AssetsServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }

    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive", false)]
        public string MonitoringServiceUrl { get; set; }
    }

    public class TransportSettings
    {
        [AmqpCheck]
        public string ClientRabbitMqConnectionString { get; set; }
    }

    public class OperationsServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
