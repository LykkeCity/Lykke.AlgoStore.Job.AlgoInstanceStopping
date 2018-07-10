﻿using Lykke.AlgoStore.Job.Stopping.Settings.JobSettings;
using Lykke.AlgoStore.Job.Stopping.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;

namespace Lykke.AlgoStore.Job.Stopping.Settings
{
    public class AppSettings
    {
        public AlgoStoreStoppingSettings AlgoStoreStoppingJob { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        [Optional]
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }
}
