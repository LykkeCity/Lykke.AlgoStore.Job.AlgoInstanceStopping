using Common.Log;
using Lykke.AlgoStore.KubernetesClient.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.KubernetesClient
{
    public class KubernetesApiClient : Kubernetes, IKubernetesApiClient
    {
        private readonly ILog _log;

        /// <summary>
        /// Initializes new instance of <see cref="KubernetesApiClient"/>
        /// </summary>
        /// <param name="baseUri">The URI of the Kubernetes instance</param>
        /// <param name="credentials">The credentials for Kubernetes instance</param>
        /// <param name="certificateHash">Certificate hash</param>
        /// <param name="userLogRepository">User log instance</param>
        public KubernetesApiClient(
            System.Uri baseUri,
            ServiceClientCredentials credentials,
            string certificateHash,
            ILog log)
            : base(baseUri, credentials, new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    return cert.GetCertHashString() == certificateHash;
                }
            })
        {
            _log = log;
        }

        /// <summary>
        /// Lists the pods by algo identifier asynchronous.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns></returns>
        public async Task<IList<Iok8skubernetespkgapiv1Pod>> ListPodsByInstanceIdAsync(string instanceId)
        {
            using (var kubeResponse =
                await ListCoreV1PodForAllNamespacesWithHttpMessagesAsync(
                    fieldSelector: "metadata.namespace=algo-test",
                    labelSelector: (string.IsNullOrEmpty(instanceId) ? null : $"app={instanceId}")))
            {
                if (!kubeResponse.Response.IsSuccessStatusCode || kubeResponse.Body == null ||
                    kubeResponse.Body.Items == null)
                    return null;
                return kubeResponse.Body.Items;
            }
        }

        /// <summary>
        /// Deletes the service and deployment asynchronous.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="namespaceParameter">The name-space identifier.</param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string instanceId, string namespaceParameter)
        {
            try
            {
                await DeleteServiceAsync(instanceId, namespaceParameter);
                return await DeletePodAsync(instanceId, namespaceParameter);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(KubernetesApiClient), nameof(DeleteAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Deletes the pod asynchronous.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="namespaceParameter">The pod.</param>
        /// <returns></returns>
        public async Task<bool> DeletePodAsync(string instanceId, string namespaceParameter)
        {
            var options = new Iok8sapimachinerypkgapismetav1DeleteOptions
            {
                PropagationPolicy = "Foreground"
            };

            using (var kubeResponse =
                await DeleteAppsV1PodsWithHttpMessagesAsync(options, instanceId, namespaceParameter))
            {
                if (!kubeResponse.Response.IsSuccessStatusCode || kubeResponse.Body == null)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Deletes the service asynchronous.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="namespaceParameter">The name-space identifier.</param>
        /// <returns></returns>
        public async Task<bool> DeleteServiceAsync(string instanceId, string namespaceParameter)
        {
            var serviceName = $"pod-{instanceId}";

            using (var kubeResponse =
                await DeleteCoreV1NSServiceWithHttpMessagesAsync(serviceName, namespaceParameter))
            {
                if (!kubeResponse.Response.IsSuccessStatusCode || kubeResponse.Body == null)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Deletes the pod with HTTP messages asynchronous.
        /// this is modification of auto generated to avoid deserialization exception
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="name">The name.</param>
        /// <param name="namespaceParameter">The namespace parameter.</param>
        /// <returns></returns>
        /// <exception cref="ValidationException">
        /// body
        /// or
        /// name
        /// or
        /// namespaceParameter
        /// </exception>
        /// <exception cref="SerializationException">Unable to deserialize the response.</exception>
        private async Task<HttpOperationResponse<string>> DeleteAppsV1PodsWithHttpMessagesAsync(Iok8sapimachinerypkgapismetav1DeleteOptions body, string name, string namespaceParameter)
        {
            if (body == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "body");
            }
            if (name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "name");
            }
            if (namespaceParameter == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "namespaceParameter");
            }

            // Construct URL
            var baseUrl = BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "api/v1/namespaces/{namespace}/pods/{name}").ToString();
            url = url.Replace("{name}", System.Uri.EscapeDataString(name));
            url = url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            List<string> _queryParameters = new List<string>();
            if (_queryParameters.Count > 0)
            {
                url += "?" + string.Join("&", _queryParameters);
            }
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            httpRequest.Method = new HttpMethod("DELETE");
            httpRequest.RequestUri = new System.Uri(url);

            // Serialize Request
            string _requestContent = null;
            if (body != null)
            {
                _requestContent = SafeJsonConvert.SerializeObject(body, SerializationSettings);
                httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }

            if (Credentials != null)
            {
                var cancellationToken = default(CancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            }

            _httpResponse = await HttpClient.SendAsync(httpRequest, default(CancellationToken)).ConfigureAwait(false);
            HttpStatusCode statusCode = _httpResponse.StatusCode;
            string responseContent = null;
            if ((int)statusCode != 200 && (int)statusCode != 401)
            {
                var ex = new HttpOperationException(string.Format("Operation returned an invalid status code '{0}'", statusCode));
                if (_httpResponse.Content != null)
                {
                    responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    responseContent = string.Empty;
                }
                ex.Request = new HttpRequestMessageWrapper(httpRequest, _requestContent);
                ex.Response = new HttpResponseMessageWrapper(_httpResponse, responseContent);
                httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            var _result = new HttpOperationResponse<string>();
            _result.Request = httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)statusCode == 200)
            {
                responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = responseContent;
                }
                catch (JsonException ex)
                {
                    httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
                }
            }

            return _result;
        }

        /// <summary>
        /// Deletes the service with HTTP messages asynchronous.
        /// this is modification of auto generated to avoid deserialization exception
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="namespaceParameter">The namespace parameter.</param>
        /// <returns></returns>
        /// <exception cref="ValidationException">
        /// name
        /// or
        /// namespaceParameter
        /// </exception>
        /// <exception cref="SerializationException">Unable to deserialize the response.</exception>
        private async Task<HttpOperationResponse<string>> DeleteCoreV1NSServiceWithHttpMessagesAsync(string name, string namespaceParameter)
        {
            if (name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "name");
            }
            if (namespaceParameter == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Construct URL
            var baseUrl = BaseUri.AbsoluteUri;
            var url = new System.Uri(new System.Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/")), "api/v1/namespaces/{namespace}/services/{name}").ToString();
            url = url.Replace("{name}", System.Uri.EscapeDataString(name));
            url = url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            List<string> queryParameters = new List<string>();
            if (queryParameters.Count > 0)
            {
                url += "?" + string.Join("&", queryParameters);
            }
            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage();
            HttpResponseMessage httpResponse = null;
            httpRequest.Method = new HttpMethod("DELETE");
            httpRequest.RequestUri = new System.Uri(url);

            if (Credentials != null)
            {
                var cancellationToken = default(CancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await Credentials.ProcessHttpRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            }

            // Serialize Request
            string requestContent = null;
            // Send Request
            httpResponse = await HttpClient.SendAsync(httpRequest, default(CancellationToken)).ConfigureAwait(false);
            HttpStatusCode statusCode = httpResponse.StatusCode;
            string responseContent = null;
            // Create Result
            var result = new HttpOperationResponse<string>();
            result.Request = httpRequest;
            result.Response = httpResponse;
            // Deserialize Response
            responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.Body = responseContent;
            }
            catch (JsonException ex)
            {
                httpRequest.Dispose();
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
                throw new SerializationException("Unable to deserialize the response.", responseContent, ex);
            }
            return result;
        }

        /// <summary>
        /// Deletes the replica set with HTTP messages asynchronous.
        /// Not used - when delete deployment it cascade delete it
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="name">The name.</param>
        /// <param name="namespaceParameter">The namespace parameter.</param>
        /// <returns></returns>
        /// <exception cref="ValidationException">
        /// body
        /// or
        /// name
        /// or
        /// namespaceParameter
        /// </exception>
        /// <exception cref="SerializationException">Unable to deserialize the response.</exception>
        private async Task<HttpOperationResponse<Iok8sapimachinerypkgapismetav1Status>> DeleteExtensionsV1beta1NSReplicaSetWithHttpMessagesAsync(Iok8sapimachinerypkgapismetav1DeleteOptions body, string name, string namespaceParameter)
        {
            if (body == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "body");
            }
            if (name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "name");
            }
            if (namespaceParameter == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "namespaceParameter");
            }
            // Construct URL
            var _baseUrl = BaseUri.AbsoluteUri;
            var _url = new System.Uri(new System.Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "apis/extensions/v1beta1/namespaces/{namespace}/replicasets/{name}").ToString();
            _url = _url.Replace("{name}", System.Uri.EscapeDataString(name));
            _url = _url.Replace("{namespace}", System.Uri.EscapeDataString(namespaceParameter));
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("DELETE");
            _httpRequest.RequestUri = new System.Uri(_url);

            // Serialize Request
            string _requestContent = null;
            if (body != null)
            {
                _requestContent = SafeJsonConvert.SerializeObject(body, SerializationSettings);
                _httpRequest.Content = new StringContent(_requestContent, System.Text.Encoding.UTF8);
                _httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }
            if (Credentials != null)
            {
                var cancellationToken = default(CancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await Credentials.ProcessHttpRequestAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            }

            _httpResponse = await HttpClient.SendAsync(_httpRequest, default(CancellationToken)).ConfigureAwait(false);
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            string _responseContent = null;
            if ((int)_statusCode != 200 && (int)_statusCode != 401)
            {
                var ex = new HttpOperationException(string.Format("Operation returned an invalid status code '{0}'", _statusCode));
                if (_httpResponse.Content != null)
                {
                    _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }
                ex.Request = new HttpRequestMessageWrapper(_httpRequest, _requestContent);
                ex.Response = new HttpResponseMessageWrapper(_httpResponse, _responseContent);
                _httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            var _result = new HttpOperationResponse<Iok8sapimachinerypkgapismetav1Status>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = SafeJsonConvert.DeserializeObject<Iok8sapimachinerypkgapismetav1Status>(_responseContent, DeserializationSettings);
                }
                catch (JsonException ex)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new SerializationException("Unable to deserialize the response.", _responseContent, ex);
                }
            }
            return _result;
        }
    }
}
