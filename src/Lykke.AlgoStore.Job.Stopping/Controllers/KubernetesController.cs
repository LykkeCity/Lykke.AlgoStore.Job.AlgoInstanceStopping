using System;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Enumerators;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.Stopping.Infrastructure;
using Lykke.AlgoStore.Job.Stopping.Models.Kubernetes;
using Lykke.AlgoStore.KubernetesClient;
using Lykke.AlgoStore.Security.InstanceAuth;
using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.AlgoStore.Job.Stopping.Core.Services;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;

namespace Lykke.AlgoStore.Job.Stopping.Controllers
{
    [Authorize]
    [Route("api/kubernetes")]
    [Produces("application/json")]
    public class KubernetesController : Controller
    {
        private IKubernetesApiClient _kubernetesApiClient;
        private IAlgoClientInstanceRepository _algoInstanceRepository;
        private IStatisticsService _statisticsService;

        public KubernetesController(IKubernetesApiClient kubernetesApiClient,
            IAlgoClientInstanceRepository algoInstanceRepository,
            IStatisticsService statisticsService)         
        {
            _kubernetesApiClient = kubernetesApiClient;
            _algoInstanceRepository = algoInstanceRepository;
            _statisticsService = statisticsService;
        }

        /// <summary>
        /// Get pods by instance id
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        [HttpGet("getPods")]
        [SwaggerOperation("GetPods")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(IEnumerable<PodResponseModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPods([FromQuery] string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return BadRequest(ErrorResponse.Create(Phrases.InstanceIdRequired));

            var pods = await _kubernetesApiClient.ListPodsByInstanceIdAsync(instanceId);

            if (pods == null || pods.Count() == 0)
                return NotFound();

            return Ok(pods.Select(p => PodResponseModel.Create(p)));
        }

        /// <summary>
        /// Delete algo instance from kubernetes by instanceId
        /// </summary>
        /// <param name="instanceId"></param>
        [HttpDelete("deleteAlgoInstance")]
        [SwaggerOperation("DeleteAlgoInstacne")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteAlgoInstances([FromQuery]string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return BadRequest(ErrorResponse.Create(Phrases.InstanceIdRequired));

            var pods = await _kubernetesApiClient.ListPodsByInstanceIdAsync(instanceId);

            if (pods == null || pods.Count() == 0 || pods[0] == null)
                return BadRequest(ErrorResponse.Create(Phrases.PodNotFound));

            var result = await _kubernetesApiClient.DeleteAsync(instanceId, pods[0].Metadata.NamespaceProperty);

            if (!result)
                return BadRequest(ErrorResponse.Create(Phrases.UnsuccessfulDeletion));

            var instanceData = await _algoInstanceRepository.GetAlgoInstanceDataByAuthTokenAsync(User.GetAuthToken());
            if (instanceData != null)
            {
                await ChangeAlgoInstanceStatusToStopped(instanceData);
                await _statisticsService.UpdateSummaryStatisticsAsync(instanceData.ClientId, instanceData.InstanceId);
            }

            return Ok();
        }

        /// <summary>
        /// Delete algo instance deployment by instance id and pod namespace
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="podNamespace"></param>
        [HttpDelete("deleteByInstanceIdAndPod")]
        [SwaggerOperation("DeleteAlgoInstacneByInstanceIdAndPod")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteAlgoInstacneByInstanceIdAndPod([FromQuery]string instanceId, [FromQuery]string podNamespace)
        {
            if (string.IsNullOrEmpty(instanceId))
                return BadRequest(ErrorResponse.Create(Phrases.InstanceIdRequired));

            if (string.IsNullOrEmpty(podNamespace))
                return BadRequest(ErrorResponse.Create(Phrases.PodNamespaceRequired));

            var result = await _kubernetesApiClient.DeleteAsync(instanceId, podNamespace);

            if (!result)
                return BadRequest(ErrorResponse.Create(Phrases.UnsuccessfulDeletion));

            var instanceData = await _algoInstanceRepository.GetAlgoInstanceDataByAuthTokenAsync(User.GetAuthToken());
            if (instanceData != null)
            {
                await ChangeAlgoInstanceStatusToStopped(instanceData);
                await _statisticsService.UpdateSummaryStatisticsAsync(instanceData.ClientId, instanceData.InstanceId);
            }

            return Ok();
        }

        private async Task ChangeAlgoInstanceStatusToStopped(AlgoClientInstanceData instanceData)
        {           
            instanceData.AlgoInstanceStatus = AlgoInstanceStatus.Stopped;
            instanceData.AlgoInstanceStopDate = DateTime.UtcNow;
            await _algoInstanceRepository.SaveAlgoInstanceDataAsync(instanceData);
        }
    }
}
