namespace Lykke.AlgoStore.Job.Stopping.Settings.JobSettings
{
    public class AlgoStoreStoppingSettings
    {
        public DbSettings Db { get; set; }
        public KubernetesSettings Kubernetes { get; set; }
    }
}
