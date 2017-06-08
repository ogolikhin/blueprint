using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ArtifactStore.Services.Workflow;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class WorkflowControllerTests
    {
        private Mock<IWorkflowService> _workflowServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _workflowServiceMock = new Mock<IWorkflowService>();
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetTransitionsAsync_InvalidStateId_ThrowsException()
        {
            var controller = new WorkflowController(_workflowServiceMock.Object);

            await controller.GetTransitionsAsync(1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetTransitionsAsync_InvalidWorkflowId_ThrowsException()
        {
            var controller = new WorkflowController(_workflowServiceMock.Object);

            await controller.GetTransitionsAsync(1, 0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task ChangeStateForArtifactAsync_InvalidArtifactId_ThrowsException()
        {
            var controller = new WorkflowController(_workflowServiceMock.Object);

            await controller.ChangeStateForArtifactAsync(0, new WorkflowStateChangeParameter());
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task ChangeStateForArtifactAsync_WorkflowStateChangeParameter_ThrowsException()
        {
            var controller = new WorkflowController(_workflowServiceMock.Object);

            await controller.ChangeStateForArtifactAsync(1, null);
        }
    }
}
