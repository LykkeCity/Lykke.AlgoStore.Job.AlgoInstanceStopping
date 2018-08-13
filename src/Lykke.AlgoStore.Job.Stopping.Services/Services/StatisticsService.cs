using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.Stopping.Core.Services;
using Lykke.AlgoStore.Job.Stopping.Services.Infrastructure.Extensions;
using Lykke.AlgoStore.Service.Statistics.Client;
using System;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Services.Services
{
    public class StatisticsService : IStatisticsService
    {
        private const string _loggingContext = "Update Summary Statistics";

        private readonly IAlgoClientInstanceRepository _algoClientInstanceRepository;
        private readonly string _statisticsServiceUrl;
        private readonly ILog _log;        

        public StatisticsService(IAlgoClientInstanceRepository algoClientInstanceRepository, string statisticsServiceUrl, ILog log)
        {
            _algoClientInstanceRepository = algoClientInstanceRepository;
            _log = log;
            _statisticsServiceUrl = statisticsServiceUrl;
        }

        public async Task UpdateSummaryStatisticsAsync(string clientId, string instanceId)
        {
            try
            {
                var instanceData =
                    await _algoClientInstanceRepository.GetAlgoInstanceDataByClientIdAsync(clientId, instanceId);
                var statisticsClient = HttpClientGeneratorHelper.GenerateClient<IStatisticsClient>(instanceData.AuthToken, _statisticsServiceUrl);

                await statisticsClient.UpdateSummaryAsync(clientId, instanceId);
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
