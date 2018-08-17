﻿using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Enumerators;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.Stopping.Settings.JobSettings;
using Lykke.AlgoStore.KubernetesClient;
using Lykke.AlgoStore.KubernetesClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.AlgoStore.Service.Statistics.Client;
using Newtonsoft.Json.Linq;
using Lykke.AlgoStore.Service.Logging.Client;
using Lykke.Service.Logging.Client.AutorestClient.Models;
using Lykke.AlgoStore.Job.Stopping.Core.Services;

namespace Lykke.AlgoStore.Job.Stopping
{
    public class ExpiredInstancesMonitor
    {
        private const string _loggingContext = "Check for expired instances";

        private readonly IAlgoClientInstanceRepository _algoClientInstanceRepository;
        private readonly IKubernetesApiClient _kubernetesApiClient;
        private readonly ILoggingClient _loggingClient;
        private readonly ExpiredInstancesMonitorSettings _settings;
        private readonly IStatisticsService _statisticsService;
        private readonly ILog _log;

        public ExpiredInstancesMonitor(
            IAlgoClientInstanceRepository algoClientInstanceRepository,
            IKubernetesApiClient kubernetesApiClient,
            ILoggingClient loggingClient,
            ExpiredInstancesMonitorSettings settings,
            IStatisticsService statisticsService,
            ILog log)
        {
            _algoClientInstanceRepository = algoClientInstanceRepository;
            _kubernetesApiClient = kubernetesApiClient;
            _loggingClient = loggingClient;
            _settings = settings;
            _statisticsService = statisticsService;
            _log = log;
        }

        public async Task StartAsync()
        {
            await ExecuteEvery();
        }

        private async Task ExecuteEvery()
        {
            while (true)
            {
                var expiredInstances = await GetExpiredAlgoInstancesAsync();

                await TryStopExpiredInstances(expiredInstances);

                var erroredPods = await GetErroredPodsAsync();

                await TryStopErroredPods(erroredPods);

                await Task.Delay(TimeSpan.FromSeconds(_settings.CheckIntervalInSeconds));
            }
        }

        public async Task TryStopErroredPods(List<Iok8skubernetespkgapiv1Pod> erroredPods)
        {
            foreach (var pod in erroredPods)
            {
                var algoInstanceParams = JObject.Parse(pod.Spec.Containers[0].Env.FirstOrDefault(e => e.Name == "ALGO_INSTANCE_PARAMS").Value);

                var instanceId = pod.Metadata.Labels["app"];
                var algoId = algoInstanceParams["AlgoId"].Value<string>();
                var algoInstance = await _algoClientInstanceRepository.GetAlgoInstanceDataByAlgoIdAsync(algoId, instanceId);
                var terminatedReason = pod.Status.ContainerStatuses[0].State.Terminated.Reason;

                var deleted = await DeleteInstancePodAsync(
                        new AlgoInstanceStoppingData { ClientId = algoInstance.ClientId, InstanceId = instanceId },
                        pod);

                if (string.IsNullOrEmpty(algoInstance.InstanceId))
                {
                    await _log.WriteWarningAsync(nameof(ExpiredInstancesMonitor), _loggingContext,
                        $"Found errored pod for deleted instance {instanceId}, algo {algoId}");
                    continue;
                }

                if (!deleted)
                {
                    await _log.WriteWarningAsync(nameof(ExpiredInstancesMonitor), _loggingContext,
                        $"Unable to stop kubernetes pod for Instance {algoInstance.InstanceId} of client id {algoInstance.ClientId}.");

                    continue;
                }

                algoInstance.AlgoInstanceStatus = terminatedReason == "Completed" ? AlgoInstanceStatus.Stopped : AlgoInstanceStatus.Errored;
                algoInstance.AlgoInstanceStopDate = DateTime.UtcNow;

                await _algoClientInstanceRepository.SaveAlgoInstanceDataAsync(algoInstance);
                await _statisticsService.UpdateSummaryStatisticsAsync(algoInstance.ClientId, algoInstance.InstanceId);

                if (terminatedReason == "OOMKilled")
                {
                    await _loggingClient.WriteAsync(new UserLogRequest
                    {
                        Date = DateTime.UtcNow,
                        InstanceId = instanceId,
                        Message = "Your instance was stopped because it ran out of resources"
                    }, algoInstance.AuthToken);
                }
            }
        }

        public async Task TryStopExpiredInstances(List<AlgoInstanceStoppingData> expiredInstances)
        {
            foreach (var instance in expiredInstances)
            {
                var instancePod = await GetInstancePodAsync(instance.InstanceId);
                if (instancePod == null)
                {
                    await _log.WriteWarningAsync(nameof(ExpiredInstancesMonitor), _loggingContext,
                        $"Instance {instance.InstanceId} of client id {instance.ClientId} is marked as started in db, but its pod was not found in kubernetеs. Will set instance status in db to Stopped.");

                    var markAsStoppedSucceded = await MarkInstanceAsStoppedInDbAsync(instance);

                    if (markAsStoppedSucceded)
                        await _statisticsService.UpdateSummaryStatisticsAsync(instance.ClientId, instance.InstanceId);

                    continue;
                }

                var deleted = await DeleteInstancePodAsync(instance, instancePod);
                if (deleted)
                {
                    await MarkInstanceAsStoppedInDbAsync(instance);
                    await _statisticsService.UpdateSummaryStatisticsAsync(instance.ClientId, instance.InstanceId);
                }
                else
                {
                    await _log.WriteWarningAsync(nameof(ExpiredInstancesMonitor), _loggingContext,
                        $"Unable to stop kubernetes pod for Instance {instance.InstanceId} of client id {instance.ClientId}.");
                }
            }
        }

        public async Task<List<AlgoInstanceStoppingData>> GetExpiredAlgoInstancesAsync()
        {
            var instances = await _algoClientInstanceRepository.GetAllAlgoInstancesPastEndDate(DateTime.UtcNow);
            return instances.Where(i => i.AlgoInstanceStatus == AlgoInstanceStatus.Started).ToList();
        }

        public async Task<List<Iok8skubernetespkgapiv1Pod>> GetErroredPodsAsync()
        {
            var pods = await _kubernetesApiClient.ListPodsByInstanceIdAsync(null);

            return pods.Where(p => p.Status.ContainerStatuses.Count == 1 
                                && p.Status.ContainerStatuses[0].State.Terminated != null).ToList();
        }

        public async Task<Iok8skubernetespkgapiv1Pod> GetInstancePodAsync(string instanceId)
        {
            var pods = await _kubernetesApiClient.ListPodsByInstanceIdAsync(instanceId);

            if (pods == null || pods.Count() == 0 || pods[0] == null)
                return null;

            return pods[0];
        }

        public async Task<bool> DeleteInstancePodAsync(AlgoInstanceStoppingData stoppingInstance,
            Iok8skubernetespkgapiv1Pod instancePod)
        {
            var deleted = await _kubernetesApiClient.DeleteAsync(stoppingInstance.InstanceId,
                instancePod.Metadata.NamespaceProperty);
            if (deleted)
            {
                await _log.WriteInfoAsync(nameof(ExpiredInstancesMonitor), _loggingContext,
                    $"Successfully stopped instance pod for instance id {stoppingInstance.InstanceId} of client id {stoppingInstance.ClientId}");
                return true;
            }
            else
            {
                await _log.WriteWarningAsync(nameof(ExpiredInstancesMonitor), _loggingContext,
                    $"Could not stop pod in kubernetеs for expired instance id {stoppingInstance.InstanceId} of client id {stoppingInstance.ClientId}");
                return false;
            }
        }

        public async Task<bool> MarkInstanceAsStoppedInDbAsync(AlgoInstanceStoppingData stoppingInstance)
        {
            var instance =
                await _algoClientInstanceRepository.GetAlgoInstanceDataByClientIdAsync(stoppingInstance.ClientId,
                    stoppingInstance.InstanceId);
            if (instance == null || instance.InstanceId == null || instance.AlgoId == null ||
                string.IsNullOrEmpty(instance.AuthToken))
            {
                await _log.WriteWarningAsync(nameof(ExpiredInstancesMonitor), _loggingContext,
                    $"Instance id {stoppingInstance.InstanceId} of client id {stoppingInstance.ClientId} not found in db and cannot be marked as stopped.");
                return false;
            }

            instance.AlgoInstanceStopDate = DateTime.UtcNow;
            instance.AlgoInstanceStatus = AlgoInstanceStatus.Stopped;
            await _algoClientInstanceRepository.SaveAlgoInstanceDataAsync(instance);
            await _log.WriteInfoAsync(nameof(ExpiredInstancesMonitor), _loggingContext,
                $"Successfully stopped instance pod for instance id {instance.InstanceId} of client id {instance.ClientId}");
            return true;
        }
    }
}
