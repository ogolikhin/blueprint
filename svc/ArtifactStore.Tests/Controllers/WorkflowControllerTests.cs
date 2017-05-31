using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ArtifactStore.Repositories.Workflow;
using ServiceLibrary.Exceptions;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class WorkflowControllerTests
    {
        private Mock<IWorkflowRepository> _sqlWorkFlowRepositoryMock;

        [TestInitialize]
        public void Setup()
        {
            _sqlWorkFlowRepositoryMock = new Mock<IWorkflowRepository>();
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
