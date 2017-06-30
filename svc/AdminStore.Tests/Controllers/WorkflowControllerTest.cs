using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Helpers;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [TestClass]
    public class WorkflowControllerTest
    {

        private Mock<IWorkflowRepository> _workflowRepoMock;
        private Mock<IServiceLogRepository> _logMock;
        private Mock<IPrivilegesRepository> _privilegesRepositoryMock;

        private WorkflowController _controller;
        private const int SessionUserId = 1;
        private const int WorkflowId = 1;

        [TestInitialize]
        public void Initialize()
        {
            _privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _workflowRepoMock = new Mock<IWorkflowRepository>();

            var session = new Session { UserId = SessionUserId };
            _controller = new WorkflowController(_workflowRepoMock.Object, _logMock.Object, _privilegesRepositoryMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            _controller.Request.RequestUri = new Uri("http://localhost");
        }

        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new WorkflowController();

            // Assert
            Assert.IsInstanceOfType(controller._privilegesManager, typeof(PrivilegesManager));
        }

        #endregion



        #region GetWorkflow

        [TestMethod]
        public async Task GetWorkflow_AllParamsAreCorrectAndPermissionsOk_ReturnWorkflow()
        {
            //arrange
            var workflow = new DWorkflow { Name = "Workflow1", Description = "DescriptionWorkflow1", Active = true};
            _workflowRepoMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            //act
            var result = await _controller.GetWorkflow(WorkflowId) as OkNegotiatedContentResult<DWorkflow>;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(workflow, result.Content);
        }

        [TestMethod]
        public async Task GetWorkflow_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            //arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            //act
            try
            {
                await _controller.GetWorkflow(WorkflowId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        [TestMethod]
        public async Task GetWorkflow_ThereIsNoSuchWorkflow_NotFoundResult()
        {
            //arrange
            ResourceNotFoundException exception = null;
            _workflowRepoMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(null);
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            //act
            try
            {
                await _controller.GetWorkflow(WorkflowId);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.WorkflowNotExist, exception.Message);
        }

        #endregion
    }
}
