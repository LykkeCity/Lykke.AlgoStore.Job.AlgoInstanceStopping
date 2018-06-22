using Lykke.SettingsReader.Attributes;

namespace Lykke.AlgoStore.Job.Stopping.Settings
{
    public class LoggingServiceSettings
    {
        [HttpCheck("api/isalive", false)]
        public string ServiceUrl { get; set; }
    }
}
