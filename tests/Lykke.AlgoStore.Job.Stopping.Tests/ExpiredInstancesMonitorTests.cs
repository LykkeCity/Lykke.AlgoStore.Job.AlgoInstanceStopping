using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.Stopping.Core.Services;
using Lykke.AlgoStore.Job.Stopping.Settings.JobSettings;
using Lykke.AlgoStore.KubernetesClient;
using Lykke.AlgoStore.KubernetesClient.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Tests
{
    [TestFixture]
    public class ExpiredInstancesMonitorTests
    {
        private const string InstanceId1 = "I1";
        private const string ClientId1 = "C1";
        private const string PodNamespace = "TESTNS";
        private ExpiredInstancesMonitor defaultMonitorMock;
        private IAlgoClientInstanceRepository defaultRepoMock;
        private IKubernetesApiClient defaultKuberClient;

        [SetUp]
        public void SetUp()
        {
            defaultRepoMock = GetAlgoClientInstanceRepositoryMock();
            defaultKuberClient = GetKubernetesApiClientMock();
            var statisticsServiceMock = GetStatisticsServiceMock();

            defaultMonitorMock = new ExpiredInstancesMonitor(defaultRepoMock, defaultKuberClient, null, GetMockSettings(), statisticsServiceMock, GetMockLog());
        }

        [Test]
        public void GetExpiredAlgoInstancesAsync_ExpectTwo_Test()
        {
            var startedAndExpiredInstancesInDb = defaultMonitorMock.GetExpiredAlgoInstancesAsync().Result;
            Assert.IsTrue(startedAndExpiredInstancesInDb.Count() == 2);
        }

        [Test]
        public void GetInstancePodAsync_ExpectOne_Test()
        {
            var pod = defaultMonitorMock.GetInstancePodAsync(InstanceId1).Result;
            Assert.IsTrue(pod!=null);
        }
        [Test]
        public void GetInstancePodAsync_ExpectNone_Test()
        {
            var pod = defaultMonitorMock.GetInstancePodAsync("non-existing").Result;
            Assert.IsTrue(pod == null);
        }

        [Test]
        public void DeleteInstancePodAsync_True_Test()
        {
            var stoppingInstance = new AlgoInstanceStoppingData { InstanceId = InstanceId1 };
            var instancePod = new Iok8skubernetespkgapiv1Pod { Metadata = new Iok8sapimachinerypkgapismetav1ObjectMeta { NamespaceProperty = PodNamespace } };

            var result = defaultMonitorMock.DeleteInstancePodAsync(stoppingInstance, instancePod).Result;
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteInstancePodAsync_False_Test()
        {
            var stoppingInstance = new AlgoInstanceStoppingData { InstanceId = "non-existing" };
            var instancePod = new Iok8skubernetespkgapiv1Pod { Metadata = new Iok8sapimachinerypkgapismetav1ObjectMeta { NamespaceProperty = "non-existing" } };

            var result = defaultMonitorMock.DeleteInstancePodAsync(stoppingInstance, instancePod).Result;
            Assert.IsFalse(result);
        }

        [Test]
        public void MarkInstanceAsStoppedInDbAsync_False_Test()
        {
            var result = defaultMonitorMock.MarkInstanceAsStoppedInDbAsync(new AlgoInstanceStoppingData()).Result;
            Assert.IsFalse(result);
        }

        [Test]
        public async Task TryStopExpiredInstances_Test()
        {
            var expiredInstances = GetMockListOfExpiredInstances().ToList();
            var task = defaultMonitorMock.TryStopExpiredInstances(expiredInstances);
            await task;
            Assert.IsTrue(task.IsCompleted == true);
        }

        private ILog GetMockLog()
        {
            return new Mock<ILog>().Object;
        }
        private ExpiredInstancesMonitorSettings GetMockSettings()
        {
            return new Mock<ExpiredInstancesMonitorSettings>().Object;
        }        

        private IKubernetesApiClient GetKubernetesApiClientMock()
        {
            var kubernetesApiClient = new Mock<IKubernetesApiClient>();

            kubernetesApiClient.Setup(a => a.ListPodsByInstanceIdAsync(It.IsAny<string>()))
                 .Returns<string>((instanceId) =>
                 {
                     var allPods = GetMockListOfKubernetisPods().ToList();
                     var found = allPods.FirstOrDefault(p => p.Metadata.Name == instanceId);
                     IList<Iok8skubernetespkgapiv1Pod> test = found!=null ? new List<Iok8skubernetespkgapiv1Pod> { found } : new List<Iok8skubernetespkgapiv1Pod>();
                     return Task.FromResult(test);
                 });

            kubernetesApiClient.Setup(a=>a.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((instanceId, nameSpace) =>
                {
                    if (instanceId == InstanceId1 && nameSpace == PodNamespace)
                        return Task.FromResult(true);
                    return Task.FromResult(false);
                });         

            return kubernetesApiClient.Object;
        }



        private IAlgoClientInstanceRepository GetAlgoClientInstanceRepositoryMock()
        {
            var algoClientInstanceRepository = new Mock<IAlgoClientInstanceRepository>();

            algoClientInstanceRepository.Setup(a => a.GetAllAlgoInstancesPastEndDate(It.IsAny<DateTime>()))
                .Returns(Task.FromResult(GetMockListOfExpiredInstances()));


            algoClientInstanceRepository.Setup(a => a.GetAlgoInstanceDataByClientIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((clientId, instanceId) =>
                {
                    var instances = GetMockListOfExpiredInstances().ToList();
                    var found = instances.FirstOrDefault(i => i.ClientId == clientId && i.InstanceId == instanceId);
                    return Task.FromResult(found ==null ? default(AlgoClientInstanceData) : new AlgoClientInstanceData { AlgoId = found.AlgoId, ClientId = found.ClientId, InstanceId = found.InstanceId, AlgoInstanceStatus = found.AlgoInstanceStatus });
                });


            algoClientInstanceRepository.Setup(a => a.SaveAlgoInstanceDataAsync(It.IsAny<AlgoClientInstanceData>()))
                .Returns(Task.CompletedTask);


            return algoClientInstanceRepository.Object;
        }

        private IEnumerable<AlgoInstanceStoppingData> GetMockListOfExpiredInstances()
        {
            return new List<AlgoInstanceStoppingData>
            {
                new AlgoInstanceStoppingData { AlgoId = "A1", AlgoInstanceStatus = CSharp.AlgoTemplate.Models.Enumerators.AlgoInstanceStatus.Started, ClientId = "C1", EndOnDateTicks = "636659723400000000", InstanceId = InstanceId1 },
                new AlgoInstanceStoppingData { AlgoId = "A2", AlgoInstanceStatus = CSharp.AlgoTemplate.Models.Enumerators.AlgoInstanceStatus.Started, ClientId = "C2", EndOnDateTicks = "636665692200000000", InstanceId = "I2" },
                new AlgoInstanceStoppingData { AlgoId = "A3", AlgoInstanceStatus = CSharp.AlgoTemplate.Models.Enumerators.AlgoInstanceStatus.Stopped, ClientId = "C3", EndOnDateTicks = "636665692200000000", InstanceId = "I3" },
                new AlgoInstanceStoppingData { AlgoId = "A4", AlgoInstanceStatus = CSharp.AlgoTemplate.Models.Enumerators.AlgoInstanceStatus.Deploying, ClientId = "C4", EndOnDateTicks = "636665692200000000", InstanceId = "I4" }
            };
        }

        private IEnumerable<Iok8skubernetespkgapiv1Pod> GetMockListOfKubernetisPods()
        {
            return new List<Iok8skubernetespkgapiv1Pod>
            {
                new Iok8skubernetespkgapiv1Pod { Metadata = new Iok8sapimachinerypkgapismetav1ObjectMeta { NamespaceProperty = PodNamespace, Name = InstanceId1 } }
            };
        }

        private IStatisticsService GetStatisticsServiceMock()
        {
            var statisticsServiceMock = new Mock<IStatisticsService>();

            statisticsServiceMock.Setup(m => m.UpdateSummaryStatisticsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            return statisticsServiceMock.Object;
        }
    }
}
