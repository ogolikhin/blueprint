using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    [TestClass]
    public class StatusControllerHelperTests
    {

        [TestMethod]
        public async Task GetStatus_NoStatusRepos_ReturnsCorrectContent()
        {
            //Arrange
            var statusControllerHelper = GetStatusControllerHelper(new List<IStatusRepository>(), "MyService");

            //Act
            var serviceStatus = await statusControllerHelper.GetStatus();

            //Assert
            Assert.AreEqual(0, serviceStatus.StatusResponses.Count);
            Assert.AreEqual("MyService", serviceStatus.ServiceName);
            Assert.IsTrue(serviceStatus.NoErrors);
            Assert.IsNull(serviceStatus.Errors);
        }

        [TestMethod]
        public async Task GetStatus_MultipleStatusRepos_ReturnsCorrectContent()
        {
            //Arrange
            var statusRepoList = new List<IStatusRepository>();

            for (int i = 0; i < 3; i++)
            {
                statusRepoList.Add(GetStatusRepo($"MyService{i}", $"MyAccessInfo{i}", $"MyResponseMessage{i}"));
            }


            var statusControllerHelper = GetStatusControllerHelper(statusRepoList, "MyService");

            //Act
            var serviceStatus = await statusControllerHelper.GetStatus();

            //Assert
            Assert.AreEqual(3, serviceStatus.StatusResponses.Count);
            Assert.AreEqual("MyService", serviceStatus.ServiceName);
            Assert.IsTrue(serviceStatus.NoErrors);
            Assert.IsNull(serviceStatus.Errors);

            for (int i = 0; i < 3; i++)
            {
                var statusResponse = serviceStatus.StatusResponses[i];
                Assert.AreEqual($"MyAccessInfo{i}", statusResponse.AccessInfo);
                Assert.AreEqual($"MyService{i}", statusResponse.Name);
                Assert.AreEqual($"MyResponseMessage{i}", statusResponse.Result);
                Assert.IsTrue(statusResponse.NoErrors);
            }
        }

        [TestMethod]
        public async Task GetStatus_MultipleStatusReposOneException_ReturnsCorrectContent()
        {
            //Arrange
            var statusRepoList = new List<IStatusRepository>();

            Mock<IStatusRepository> statusRepoMock;
            for (int i = 0; i < 3; i++)
            {
                statusRepoList.Add(GetStatusRepo($"MyService{i}", $"MyAccessInfo{i}", $"MyResponseMessage{i}"));
            }

            statusRepoMock = new Mock<IStatusRepository>();
            statusRepoMock.Setup(r => r.GetStatuses(It.IsAny<int>())).Throws(new Exception("MyException"));
            statusRepoMock.Setup(r => r.Name).Returns($"MyService3");
            statusRepoMock.Setup(r => r.AccessInfo).Returns($"MyAccessInfo3");
            statusRepoList.Add(statusRepoMock.Object);

            var statusControllerHelper = GetStatusControllerHelper(statusRepoList, "MyService");

            //Act
            var serviceStatus = await statusControllerHelper.GetStatus();

            //Assert
            Assert.AreEqual(4, serviceStatus.StatusResponses.Count);
            Assert.AreEqual("MyService", serviceStatus.ServiceName);
            Assert.IsFalse(serviceStatus.NoErrors);
            Assert.IsNull(serviceStatus.Errors);

            for (int i = 0; i < 3; i++)
            {
                var statusResponse = serviceStatus.StatusResponses[i];
                Assert.AreEqual($"MyAccessInfo{i}", statusResponse.AccessInfo);
                Assert.AreEqual($"MyService{i}", statusResponse.Name);
                Assert.AreEqual($"MyResponseMessage{i}", statusResponse.Result);
                Assert.IsTrue(statusResponse.NoErrors);
            }

            {
                int i = 3;
                var statusResponse = serviceStatus.StatusResponses[i];
                Assert.AreEqual($"MyAccessInfo{i}", statusResponse.AccessInfo);
                Assert.AreEqual($"MyService{i}", statusResponse.Name);

                Assert.IsTrue(statusResponse.Result.Contains("ERROR: System.Exception: MyException"));
                Assert.IsFalse(statusResponse.NoErrors);
            } 
        }

        private StatusControllerHelper GetStatusControllerHelper(List<IStatusRepository> statusRepos, string serviceName)
        {
            return new StatusControllerHelper(statusRepos, serviceName, GetLogRepo(), "myLogSource");

        }

        private IServiceLogRepository GetLogRepo()
        {
            var logRepo = new Mock<IServiceLogRepository>();
            //logRepo.Setup(r => r.LogError(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(null);
            return logRepo.Object;
        }

        private IStatusRepository GetStatusRepo(string name, string accessInfo, string getStatusResponse)
        {
            var statusRepoMock = new Mock<IStatusRepository>();
            //statusRepoMock.Setup(r => r.GetStatuses(It.IsAny<int>())).ReturnsAsync(getStatusResponse);
            statusRepoMock.Setup(r => r.Name).Returns(name);
            statusRepoMock.Setup(r => r.AccessInfo).Returns(accessInfo);
            return statusRepoMock.Object;
        }

    }
}
