using Lykke.SettingsReader.Attributes;

namespace Lykke.AlgoStore.Job.Stopping.Settings
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
