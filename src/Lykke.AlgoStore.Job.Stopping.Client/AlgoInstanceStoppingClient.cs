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
        public async Task<PodsResponse> GetPodsAsync(string instanceId, string instanceAuthToken)
        {
            var response = await _apiClient.GetPodsWithHttpMessagesAsync(instanceId, SetAutorizationToken(instanceAuthToken));
            return PreparePodsResponse(response);
        }

        /// <summary>
        /// Delete instance by instance id
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public async Task<DeleteAlgoInstanceResponseModel> DeleteAlgoInstanceAsync(string instanceId, string instanceAuthToken)
        {
            var response = await _apiClient.DeleteAlgoInstacneWithHttpMessagesAsync(instanceId, SetAutorizationToken(instanceAuthToken));
            return PrepareDeleteResponse(response.Body);
        }

        /// <summary>
        /// Delete instance by instance id and pod namespace
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="pod"></param>
        /// <returns></returns>
        public async Task<DeleteAlgoInstanceResponseModel> DeleteAlgoInstanceByInstanceIdAndPodAsync(string instanceId, string podNamespace, string instanceAuthToken)
        {
            var response = await _apiClient.DeleteAlgoInstacneByInstanceIdAndPodWithHttpMessagesAsync
                                            (instanceId, podNamespace, SetAutorizationToken(instanceAuthToken));
            return PrepareDeleteResponse(response.Body);
        }

        private DeleteAlgoInstanceResponseModel PrepareDeleteResponse(ErrorResponse response)
        {
            var result = new DeleteAlgoInstanceResponseModel();

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

        /// <summary>
        /// Set autorization heaader options
        /// </summary>
        /// <param name="authToken">The authorization token that would be used</param>
        private Dictionary<string, List<string>> SetAutorizationToken(string authToken)
        {
            var result = new Dictionary<string, List<string>>();
            result.Add("Authorization", new List<string>() { "Bearer "+ authToken });

            return result;
        }
    }
}
