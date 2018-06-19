using Lykke.AlgoStore.KubernetesClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.KubernetesClient
{
    public interface IKubernetesApiClient
    {
        Task<IList<Iok8skubernetespkgapiv1Pod>> ListPodsByAlgoIdAsync(string instanceId);
        Task<bool> DeleteAsync(string instanceId, string podNamespace);
    }
}
