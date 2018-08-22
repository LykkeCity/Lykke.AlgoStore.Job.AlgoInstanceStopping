using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.Stopping.Core.Services;
using Lykke.AlgoStore.Service.Statistics.Client;
using System;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Services.Services
{
    public class StatisticsService : IStatisticsService
    {
        private const string _loggingContext = "Update Summary Statistics";

        private readonly IAlgoClientInstanceRepository _algoClientInstanceRepository;
        private readonly IStatisticsClient _statisticsClient;
        private readonly ILog _log;
        private readonly int _statisticsSummaryUpdateDelayInMilisec;

        public StatisticsService(IAlgoClientInstanceRepository algoClientInstanceRepository, IStatisticsClient statisticsClient, int statisticsSummaryUpdateDelayInMilisec, ILog log)
        {
            _algoClientInstanceRepository = algoClientInstanceRepository;
            _statisticsClient = statisticsClient;
            _statisticsSummaryUpdateDelayInMilisec = statisticsSummaryUpdateDelayInMilisec;
            _log = log;         
        }

        public async Task UpdateSummaryStatisticsAsync(string clientId, string instanceId)
        {
            try
            {
                var instanceData = await _algoClientInstanceRepository.GetAlgoInstanceDataByClientIdAsync(clientId, instanceId);

                await Task.Delay(_statisticsSummaryUpdateDelayInMilisec);

                await _statisticsClient.UpdateSummaryAsync(clientId, instanceId, instanceData.AuthToken.ToBearerToken());
            }
            catch (Exception ex)
            {
                await _log.WriteWarningAsync(nameof(UpdateSummaryStatisticsAsync),
                    _loggingContext,
                    $"Failed to update summary statistics for instance {instanceId} of client {clientId}",
                    ex);
            }
        }
    }
}
