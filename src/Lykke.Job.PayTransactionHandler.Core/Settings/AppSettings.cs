﻿using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
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
    }

    public class NinjaServiceClientSettings
    {
        [HttpCheck("/")]
        public string ServiceUrl { get; set; }
    }
}
