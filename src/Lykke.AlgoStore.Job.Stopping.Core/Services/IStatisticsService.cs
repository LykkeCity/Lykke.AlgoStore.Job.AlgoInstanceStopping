using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Core.Services
{
    public interface IStatisticsService
    {
        Task UpdateSummaryStatisticsAsync(string clientId, string instanceId);
    }
}
