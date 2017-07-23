﻿using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    [TestClass]
    public class WorkflowRepositoryTest
    {
        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflowDetailsAsync_WeHaveThisWorkflowInDb_QueryReturnWorkflow()
        {
            //arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);

            var workflowId = 10;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description" };
            var workflowsList = new List<SqlWorkflow> { workflow };
            cxn.SetupQueryAsync("GetWorkflowDetails", new Dictionary<string, object> { { "WorkflowId", workflowId } }, workflowsList);

            //act
            var workflowDetails = await repository.GetWorkflowDetailsAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region GetWorkflowArtifactTypesAndProjectsAsync

        [TestMethod]
        public async Task GetWorkflowArtifactTypesAndProjectsAsync_ThereExistWorkflowArtifactTypesAndProjects_QueryReturnWorkflowArtifactTypesAndProjects()
        {
            //arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);
            var workflowId = 10;
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

            cxn.SetupQueryAsync("GetWorkflowProjectsAndArtifactTypes", It.IsAny<Dictionary<string, object>>(), workflowArtifactTypesAndProjects);

            //act
            var workflowDetails = await repository.GetWorkflowArtifactTypesAndProjectsAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region UpdateWorkflows

        [TestMethod]
        public async Task UpdateWorkflows_UpdateThisWorkflowInDb_QueryReturnWorkflows()
        {
            //arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);
            var publishRevision = 12;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description", Active = true };
            var workflowsList = new List<SqlWorkflow> { workflow };
            cxn.SetupQueryAsync("UpdateWorkflows", It.IsAny<Dictionary<string, object>>(), workflowsList);

            //act
            var updatedWorkflows = await repository.UpdateWorkflows(workflowsList, publishRevision);

            //assert
            Assert.IsNotNull(updatedWorkflows);
        }

        #endregion

        #region GetWorkflowTransitionsAndPropertyChangesByWorkflowId

        [TestMethod]
        public async Task GetWorkflowTransitionsAndPropertyChangesByWorkflowId_ThereExistWorkflowTransitionsAndPropertyChanges_QueryReturnWorkflowTransitionsAndPropertyChanges()
        {
            //arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);
            var workflowId = 10;
            var workflowTransitionsAndPropertyChanges = new List<SqlWorkflowTransitionsAndPropertyChanges>
            {
                new SqlWorkflowTransitionsAndPropertyChanges
                {
                    WorkflowId = 10,
                    Name = "FirsTrigger",
                    FromState = "new",
                    ToState = "Active",
                    Permissions = "<P S=\"0\"><G>1</G></P>",
                    Type = 1
                },
                new SqlWorkflowTransitionsAndPropertyChanges
                {
                    WorkflowId = 10,
                    Name = "second Trigger",
                    FromState = "Active",
                    Permissions = "<P S=\"0\"/>",
                    Type = 2
                }
            };

            cxn.SetupQueryAsync("GetWorkflowTransitionsAndPropertyChangesById", It.IsAny<Dictionary<string, object>>(), workflowTransitionsAndPropertyChanges);

            //act
            var workflowDetails = await repository.GetWorkflowTransitionsAndPropertyChangesByWorkflowId(workflowId);

            //assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region GetWorkflowStatesByWorkflowId

        [TestMethod]
        public async Task GetWorkflowStatesByWorkflowId_WeHaveThisWorkflowInDb_QueryReturnWorkflowStates()
        {
            //arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);

            var workflowId = 10;
            var workflow = new SqlState { Name = "Workflow1", Default = true };
            var workflowsList = new List<SqlState> { workflow };
            cxn.SetupQueryAsync("GetWorkflowStatesById", new Dictionary<string, object> { { "WorkflowId", workflowId } }, workflowsList);

            //act
            var workflowStates = await repository.GetWorkflowStatesByWorkflowId(workflowId);

            //assert
            Assert.IsNotNull(workflowStates);
        }

        #endregion
    }
}
