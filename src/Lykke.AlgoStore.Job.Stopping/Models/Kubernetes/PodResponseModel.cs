using Lykke.AlgoStore.KubernetesClient.Models;

namespace Lykke.AlgoStore.Job.Stopping.Models.Kubernetes
{
    public class PodResponseModel
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Phase { get; set; }

        public static PodResponseModel Create(Iok8skubernetespkgapiv1Pod kubernetesPod)
        {
            return new PodResponseModel()
            {
                Name = kubernetesPod.Metadata.Name,
                Namespace = kubernetesPod.Metadata.NamespaceProperty,
                Phase = kubernetesPod.Status.Phase
            };
        }
    }
}
