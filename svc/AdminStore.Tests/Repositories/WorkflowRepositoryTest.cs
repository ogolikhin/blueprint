using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    [TestClass]
    public class WorkflowRepositoryTest
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesConnectionToRaptorMain()
        {
            // Arrange

            // Act
            var repository = new WorkflowRepository();

            // Assert
            Assert.AreEqual(ServiceConstants.RaptorMain, repository.ConnectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor


        #region GetWorkflow

        [TestMethod]
        public async Task GetWorkflow_WeHaveThisWorkflowInDb_QueryReturnWorkflow()
        {
            //arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object,
                userRepositoryMock.Object);
            
            var workflowId = 10;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description"};
            var workflowsList = new List<SqlWorkflow> {workflow};
            cxn.SetupQueryAsync("GetWorkflowDetails", new Dictionary<string, object> { { "WorkflowId", workflowId } }, workflowsList);

            //act
            var workflowDetails = await repository.GetWorkflowDetailsAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion
    }
}
