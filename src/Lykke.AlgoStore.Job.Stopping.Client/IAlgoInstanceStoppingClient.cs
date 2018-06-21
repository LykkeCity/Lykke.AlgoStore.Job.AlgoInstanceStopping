using Lykke.AlgoStore.Job.Stopping.Client.Models.ResponseModels;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Client
{
    public interface IAlgoInstanceStoppingClient
    {
        Task<PodsResponse> GetAsync(string instanceId);
        Task<DeleteAlgoInsatnceResponseModel> DeleteAlgoInstanceAsync(string instanceId);
        Task<DeleteAlgoInsatnceResponseModel> DeleteAlgoInstanceByInstanceIdAndPodAsync(string instanceId, string podNamespace);
    }
}
