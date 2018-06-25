using Lykke.AlgoStore.Job.Stopping.Client.Models.ResponseModels;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Client
{
    public interface IAlgoInstanceStoppingClient
    {
        Task<PodsResponse> GetAsync(string instanceId, string instanceAuthtoken);
        Task<DeleteAlgoInsatnceResponseModel> DeleteAlgoInstanceAsync(string instanceId, string instanceAuthtoken);
        Task<DeleteAlgoInsatnceResponseModel> DeleteAlgoInstanceByInstanceIdAndPodAsync(string instanceId, string podNamespace, string instanceAuthtoken);
    }
}
