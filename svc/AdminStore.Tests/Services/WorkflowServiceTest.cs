using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Services
{
    [TestClass]
    public class WorkflowServiceTest
    {
        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflow_WorkflowExists_ReturnWorkflow()
        {
            //arrange
            var workflowRepositoryMock = new Mock<IWorkflowRepository>();
            var workflowValidatorMock = new Mock<IWorkflowValidator>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var workflowService = new WorkflowService(workflowRepositoryMock.Object, workflowValidatorMock.Object,
                userRepositoryMock.Object);
            var workflowId = 10;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description" };
            var workflowArtifactTypesAndProjects = new List<SqlWorkflowArtifactTypesAndProjects>
            {
                new SqlWorkflowArtifactTypesAndProjects
                {
                    ProjectId = 1,
                    ProjectName = "Project1",
                    ArtifactName = "Artifact1"
                },
                new SqlWorkflowArtifactTypesAndProjects
                {
                    ProjectId = 1,
                    ProjectName = "Project1",
                    ArtifactName = "Artifact2"
                },
                new SqlWorkflowArtifactTypesAndProjects
                {
                    ProjectId = 2,
                    ProjectName = "Project2",
                    ArtifactName = "Artifact2"
                }
            };

            workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);

            workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAndProjectsAsync(It.IsAny<int>())).ReturnsAsync(workflowArtifactTypesAndProjects);

            //act
            var workflowDetails = await workflowService.GetWorkflowDetailsAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowDetails);
            Assert.AreEqual(2, workflowDetails.Projects.Count());
            Assert.AreEqual(2, workflowDetails.ArtifactTypes.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetWorkflow_ThereIsNoSuchWorkflow_NotFoundResult()
        {
            //arrange
            var workflowRepositoryMock = new Mock<IWorkflowRepository>();
            var workflowValidatorMock = new Mock<IWorkflowValidator>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var workflowService = new WorkflowService(workflowRepositoryMock.Object, workflowValidatorMock.Object,
                userRepositoryMock.Object);
            var workflowId = 10;

            workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)null);

            //act
            await workflowService.GetWorkflowDetailsAsync(workflowId);       
        }

        #endregion
    }
}
