using Lykke.AlgoStore.Job.Stopping.Client.AutorestClient.Models;
using System.Collections.Generic;

namespace Lykke.AlgoStore.Job.Stopping.Client.Models.ResponseModels
{
    public class PodsResponse
    {
        public ErrorModel Error { get; set; }
        public IList<PodResponseModel> Records { get; set; }
    }
}
