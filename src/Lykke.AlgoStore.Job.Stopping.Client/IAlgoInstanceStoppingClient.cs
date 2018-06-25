using Lykke.AlgoStore.Job.Stopping.Client.Models.ResponseModels;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Client
{
    public interface IAlgoInstanceStoppingClient
    {
        Task<PodsResponse> GetPodsAsync(string instanceId, string instanceAuthtoken);
        Task<DeleteAlgoInstanceResponseModel> DeleteAlgoInstanceAsync(string instanceId, string instanceAuthtoken);
        Task<DeleteAlgoInstanceResponseModel> DeleteAlgoInstanceByInstanceIdAndPodAsync(string instanceId, string podNamespace, string instanceAuthtoken);
    }
}
