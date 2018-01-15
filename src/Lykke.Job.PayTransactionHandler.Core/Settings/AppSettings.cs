using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.Job.PayTransactionHandler.Core.Settings.SlackNotifications;

namespace Lykke.Job.PayTransactionHandler.Core.Settings
{
    public class AppSettings
    {
        public PayTransactionHandlerSettings PayTransactionHandlerJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}