using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Entities;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.KubernetesClient;
using Lykke.AlgoStore.KubernetesClient.Models;
using Microsoft.Rest;
using NUnit.Framework;

namespace Lykke.AlgoStore.Job.Stopping.Tests
{
    [TestFixture]
    public class KubernetsClientTests
    {
        //private const string Id = "test3";
        private const string Id = "aab7cff6-8690-47d3-a0db-8e84bdad4b03";

        #region Private Methods

        private async Task<IList<Iok8skubernetespkgapiv1Pod>> When_I_Call_ListPodsByAlgoIdAsync(IKubernetesApiClient client)
        {
            return await client.ListPodsByAlgoIdAsync(Id);
        }

        private async Task<bool> When_I_Call_DeleteDeploymentAsync(KubernetesApiClient client, Iok8skubernetespkgapiv1Pod pod)
        {
            return await client.DeleteDeploymentAsync(Id, pod.Metadata.NamespaceProperty);
        }

        private async Task<bool> When_I_Call_DeleteServiceAsync(KubernetesApiClient client, Iok8skubernetespkgapiv1Pod pod)
        {
            return await client.DeleteServiceAsync(Id, pod.Metadata.NamespaceProperty);
        }

        private async Task<bool> When_I_Call_DeleteServiceAsync(KubernetesApiClient client, string namespaceParameter)
        {
            return await client.DeleteServiceAsync(Id, namespaceParameter);
        }

        private async Task<bool> When_I_Call_DeleteServiceAsync(KubernetesApiClient client, string name, string namespaceParameter)
        {
            return await client.DeleteServiceAsync(name, namespaceParameter);
        }

        private static void Then_Result_ShouldBe_Valid(IList<Iok8skubernetespkgapiv1Pod> pods)
        {
            Assert.IsNotNull(pods);
            Assert.AreEqual(1, pods.Count);
            Assert.IsNotNull(pods[0]);
        }
        private static void Then_Result_Should_Contain_LogData(string result)
        {
            Assert.IsNotNull(result);
        }
        private static void Then_Result_ShouldBe_True(bool status)
        {
            Assert.IsTrue(status);
        }
        #endregion
    }
}
