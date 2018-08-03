using Lykke.AlgoStore.Security.InstanceAuth;

namespace Lykke.AlgoStore.Job.Stopping.Settings.JobSettings
{
    public class AlgoStoreStoppingSettings
    {
        public DbSettings Db { get; set; }
        public KubernetesSettings Kubernetes { get; set; }
        public InstanceAuthSettings StoppingServiceCache { get; set; }
        public ExpiredInstancesMonitorSettings ExpiredInstancesMonitor { get; set; }
    }
}
