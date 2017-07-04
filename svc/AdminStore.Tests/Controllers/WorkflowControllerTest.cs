﻿using System;
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
using AdminStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [TestClass]
    public class WorkflowControllerTest
    {

        private Mock<IWorkflowService> _workflowServiceMock;
        private Mock<IServiceLogRepository> _logMock;
        private Mock<IPrivilegesRepository> _privilegesRepositoryMock;
        private Mock<IWorkflowRepository> _workflowRepositoryMock;

        private WorkflowController _controller;
        private const int SessionUserId = 1;
        private const int WorkflowId = 1;

        [TestInitialize]
        public void Initialize()
        {
            _privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _workflowServiceMock = new Mock<IWorkflowService>();
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();

            var session = new Session { UserId = SessionUserId };
            _controller = new WorkflowController(_workflowRepositoryMock.Object, _workflowServiceMock.Object, _logMock.Object, _privilegesRepositoryMock.Object)
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
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "DescriptionWorkflow1", Active = true };
            _workflowServiceMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            //act
            var result = await _controller.GetWorkflow(WorkflowId) as OkNegotiatedContentResult<SqlWorkflow>;

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

        #endregion

        #region GetWorkflows

        [TestMethod]
        public async Task GetWorkflows_AllParamsAreCorrectAndPermissionsOk_ReturnWorkflows()
        {
            //arrange
            var workflows = new QueryResult<WorkflowDto>() { Total = 10, Items = new List<WorkflowDto>() { new WorkflowDto(), new WorkflowDto() } };
            var pagination = new Pagination() { Limit = 10, Offset = 0 };
            var sorting = new Sorting() { Order = SortOrder.Asc, Sort = "name" };

            _workflowRepositoryMock.Setup(w => w.GetWorkflows(It.IsAny<Pagination>(), It.IsAny<Sorting>(), It.IsAny<string>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(workflows);

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            //act
            var result =
                await _controller.GetWorkflows(pagination, sorting) as OkNegotiatedContentResult<QueryResult<WorkflowDto>>;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(workflows, result.Content);
        }

        [TestMethod]
        public async Task GetWorkflows_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            //arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            //act
            try
            {
                await _controller.GetWorkflows(new Pagination(), new Sorting());
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        #endregion
    }
}
