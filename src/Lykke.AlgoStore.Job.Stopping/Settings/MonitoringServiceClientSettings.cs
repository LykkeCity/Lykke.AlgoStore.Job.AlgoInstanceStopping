using Lykke.SettingsReader.Attributes;

namespace Lykke.AlgoStore.Job.Stopping.Settings
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive", false)]
        public string MonitoringServiceUrl { get; set; }
    }
}
