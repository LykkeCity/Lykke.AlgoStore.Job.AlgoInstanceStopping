using Lykke.AlgoStore.Job.Stopping.Controllers;
using Lykke.AlgoStore.KubernetesClient;
using Lykke.AlgoStore.KubernetesClient.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.Stopping.Tests
{
    public class KubernetesControllerTests
    {
        private string testInstanceId = "2d84ba4f-6fc8-4e89-aee8-bc24ae75bde6";
        private string testPodNamespace = "test-algostore-namespce";

        [Test]
        public async Task GetPods_ReturnNotFound()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            kubernetesClientMock.Setup(k => k.ListPodsByInstanceIdAsync(It.IsAny<string>()))
                                .Returns(Task.FromResult<IList<Iok8skubernetespkgapiv1Pod>>(null));

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.GetPods(testInstanceId);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task GetPods_ReturnBadRequest()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.GetPods("");
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteAlgoInstances_ReturnBadRequest_InstanceNotValid()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.DeleteAlgoInstances("");
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteAlgoInstances_ReturnBadRequest_NotFoundPod()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            kubernetesClientMock.Setup(k => k.ListPodsByInstanceIdAsync(It.IsAny<string>()))
                                .Returns(Task.FromResult<IList<Iok8skubernetespkgapiv1Pod>>(new List<Iok8skubernetespkgapiv1Pod>()));

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.DeleteAlgoInstances(testInstanceId);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteAlgoInstances_ReturnBadRequest_UnsuccessfulDeletion()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            kubernetesClientMock.Setup(k => k.ListPodsByInstanceIdAsync(It.IsAny<string>()))
                                .Returns(Task.FromResult(GetTestPods()));

            kubernetesClientMock.Setup(k => k.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(Task.FromResult(false));

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.DeleteAlgoInstances(testInstanceId);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteAlgoInstances_SuccessfulDeletion()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            kubernetesClientMock.Setup(k => k.ListPodsByInstanceIdAsync(It.IsAny<string>()))
                                .Returns(Task.FromResult(GetTestPods()));

            kubernetesClientMock.Setup(k => k.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(Task.FromResult(true));

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.DeleteAlgoInstances(testInstanceId);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task DeleteAlgoInstances_ByPodNamespace_SuccessfulDeletion()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            kubernetesClientMock.Setup(k => k.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(Task.FromResult(true));

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.DeleteAlgoInstacneByInstanceIdAndPod(testInstanceId, testPodNamespace);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task DeleteAlgoInstances_ByPodNamespace_BadRequest()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.DeleteAlgoInstacneByInstanceIdAndPod("", "");
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Check_InstanceId_And_Pod_Are_Not_Changed()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            kubernetesClientMock.Setup(k => k.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns((string instanceId, string podNamespace) =>
                {
                    CheckIfPodIsChanged(podNamespace);
                    CheckIfInstanceIdChanged(instanceId);
                    return Task.FromResult(true);
                });

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.DeleteAlgoInstacneByInstanceIdAndPod(testInstanceId, testPodNamespace);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task Check_InstanceId_Is_Not_Changed()
        {
            var kubernetesClientMock = new Mock<IKubernetesApiClient>();

            kubernetesClientMock.Setup(k => k.ListPodsByInstanceIdAsync(It.IsAny<string>()))
                            .Returns((string instanceId) =>
                            {
                                CheckIfInstanceIdChanged(instanceId);
                                return Task.FromResult(GetTestPods());
                            });

            KubernetesController controller = new KubernetesController(kubernetesClientMock.Object);

            var result = await controller.GetPods(testInstanceId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        private IList<Iok8skubernetespkgapiv1Pod> GetTestPods()
        {
            var pods = new List<Iok8skubernetespkgapiv1Pod>();

            pods.Add(new Iok8skubernetespkgapiv1Pod()
            {
                Metadata = new Iok8sapimachinerypkgapismetav1ObjectMeta()
                {
                    NamespaceProperty = testPodNamespace
                }
            });

            return pods;
        }

        private void CheckIfPodIsChanged(string podNamespace)
        {
            Assert.AreEqual(testPodNamespace, podNamespace);
        }

        private void CheckIfInstanceIdChanged(string instanceId)
        {
            Assert.AreEqual(testInstanceId, instanceId);
        }
    }
}
