using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtifactStore.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ArtifactStore.Repositories.Workflow;
using ServiceLibrary.Exceptions;

namespace AdminStore.Controllers
{
    [TestClass]
    public class WorkflowControllerTests
    {
        private Mock<ISqlWorkflowRepository> _sqlWorkFlowRepositoryMock;

        [TestInitialize]
        public void Setup()
        {
            _sqlWorkFlowRepositoryMock = new Mock<ISqlWorkflowRepository>();
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetTransitionsAsync_InvalidStateId_ThrowsException()
        {
            var controller = new WorkflowController(_sqlWorkFlowRepositoryMock.Object);

            await controller.GetTransitionsAsync(1, 1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetTransitionsAsync_InvalidWorkflowId_ThrowsException()
        {
            var controller = new WorkflowController(_sqlWorkFlowRepositoryMock.Object);

            await controller.GetTransitionsAsync(1, 0, 1);
        }

    }
}
