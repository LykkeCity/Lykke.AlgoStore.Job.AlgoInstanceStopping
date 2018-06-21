using Lykke.AlgoStore.Job.Stopping.Client.AutorestClient;
using Lykke.AlgoStore.Job.Stopping.Client.AutorestClient.Models;
using Lykke.AlgoStore.Job.Stopping.Client.Models.ResponseModels;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Client
{
    public class AlgoInstanceStoppingClient : IAlgoInstanceStoppingClient, IDisposable
    {
        private StoppingJobAPI _apiClient;

        public AlgoInstanceStoppingClient(string serviceUrl)
        {
            _apiClient = new StoppingJobAPI(new Uri(serviceUrl));
        }

        public void Dispose()
        {
            if (_apiClient == null)
                return;

            _apiClient.Dispose();
            _apiClient = null;
        }

        /// <summary>
        /// Get pods information
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public async Task<PodsResponse> GetAsync(string instanceId)
        {
            var response = await _apiClient.GetPodsWithHttpMessagesAsync(instanceId);
            return PreparePodsResponse(response);
        }

        /// <summary>
        /// Delete instance by instance id
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public async Task<DeleteAlgoInsatnceResponseModel> DeleteAlgoInstanceAsync(string instanceId)
        {
            var response = await _apiClient.DeleteAlgoInstacneAsync(instanceId);
            return PrepareDeleteResponse(response);
        }

        /// <summary>
        /// Delete instance by instance id and pod namespace
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="pod"></param>
        /// <returns></returns>
        public async Task<DeleteAlgoInsatnceResponseModel> DeleteAlgoInstanceByInstanceIdAndPodAsync(string instanceId, string podNamespace)
        {
            var response = await _apiClient.DeleteAlgoInstacneByInstanceIdAndPodAsync(instanceId);
            return PrepareDeleteResponse(response);
        }

        private DeleteAlgoInsatnceResponseModel PrepareDeleteResponse(ErrorResponse response)
        {
            var result = new DeleteAlgoInsatnceResponseModel();

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                result.IsSuccessfulDeletion = false;
                result.ErrorMessage = response.ErrorMessage;
                return result;
            }

            result.IsSuccessfulDeletion = true;
            return result;
        }

        private PodsResponse PreparePodsResponse(HttpOperationResponse<object> serviceResponse)
        {
            var error = serviceResponse.Body as ErrorResponse;
            var result = serviceResponse.Body as IList<PodResponseModel>;

            if (error != null)
            {
                return new PodsResponse
                {
                    Error = new ErrorModel
                    {
                        ErrorMessage = error.ErrorMessage
                    }
                };
            }

            if (result != null)
            {
                return new PodsResponse
                {
                    Records = result
                };
            }

            throw new ArgumentException("Unknown response object");
        }
    }
}
