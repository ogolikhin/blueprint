using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System;
using System.Linq;
using ServiceLibrary.Exceptions;
using AdminStore.Models;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    [TestClass]
    public class WorkflowRepositoryTest
    {
        [TestInitialize]
        public void Initialize()
        {
            _sqlConnectionWrapperMock = new SqlConnectionWrapperMock();
            _sqlHelperMock = new Mock<ISqlHelper>();
            _workflowRepository = new WorkflowRepository(_sqlConnectionWrapperMock.Object, _sqlHelperMock.Object);

            WorkflowId = 1;
            Pagination = new Pagination() { Limit = int.MaxValue, Offset = 0 };

            ExpectedArtifacts = new List<WorkflowProjectArtifactsDto>
            {
                new WorkflowProjectArtifactsDto()
                {
                    ProjectId = 1,
                    ProjectName = "TestProject",
                    Artifacts = new List<WorkflowArtifact>()
                    {
                        new WorkflowArtifact()
                        {
                            Id = 1,
                            Name = "TestArtifact"
                        }
                    }
                }
            };
        }
       
        private SqlConnectionWrapperMock _sqlConnectionWrapperMock;
        private WorkflowRepository _workflowRepository;
        private Mock<ISqlHelper> _sqlHelperMock;
        private WorkflowAssignScope _workflowAssignScope;
        private int _workflowId;

        #region AssignProjectsAndArtifactsToWorkflow
        [TestMethod]
        public async Task AssignProjectsAndArtifactsToWorkflow_ExistsAssignedProjects_QueryReturnCountAssignedProjects()
        {
            //arrange    
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactsToWorkflow", It.IsAny<Dictionary<string, object>>(), 2);

            //act
            var result = await _workflowRepository.AssignProjectsAndArtifactsToWorkflow(_workflowId, _workflowAssignScope);

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactsToWorkflow_WorkflowByWorkflowIdIsActive_ReturnConflictExceptionE()
        {
            //arrange      
            Exception exception = null;            

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactsToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive } });
            //act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactsToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            //assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactsToWorkflow_WorkflowByWorkflowIdIsNotFound_ReturnResourceNotFoundExceptionE()
        {
            //arrange                  
            Exception exception = null;            

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactsToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist } });
            //act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactsToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            //assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactsToWorkflow_GeneralSQLException_ReturnException()
        {
            //arrange                  
            Exception exception = null;           

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactsToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.GeneralSqlError } });
            //act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactsToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            //assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }
        #endregion

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
            var workflowArtifactTypesAndProjects = new List<SqlWorkflowArtifactTypes>
            {
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 1,
                    ProjectPath = "Project1",
                    ArtifactTypeName = "Artifact1",
                    ArtifactTypeId = 205
                },
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 1,
                    ProjectPath = "Project1",
                    ArtifactTypeName = "Artifact2",
                    ArtifactTypeId = 206
                },
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 2,
                    ProjectPath = "Project1",
                    ArtifactTypeName = "Artifact2",
                    ArtifactTypeId = 206
                }
            };

            cxn.SetupQueryAsync("GetWorkflowArtifactTypesAsync", It.IsAny<Dictionary<string, object>>(), workflowArtifactTypesAndProjects);

            //act
            var workflowDetails = await repository.GetWorkflowArtifactTypesAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region UpdateWorkflowsAsync

        [TestMethod]
        public async Task UpdateWorkflowsAsync_UpdateThisWorkflowInDb_QueryReturnWorkflows()
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
            var updatedWorkflows = await repository.UpdateWorkflowsAsync(workflowsList, publishRevision);

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
            var workflowTransitionsAndPropertyChanges = new List<SqlWorkflowEventData>
            {
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "FirsTrigger",
                    FromState = "new",
                    ToState = "Active",
                    Permissions = "<P S=\"0\"><G>1</G></P>",
                    Type = 1
                },
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "second Trigger",
                    FromState = "Active",
                    Permissions = "<P S=\"0\"/>",
                    Type = 2
                }
            };

            cxn.SetupQueryAsync("GetWorkflowStatesById", It.IsAny<Dictionary<string, object>>(), workflowTransitionsAndPropertyChanges);

            //act
            var workflowDetails = await repository.GetWorkflowEventsAsync(workflowId);

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
            var workflowStates = await repository.GetWorkflowStatesAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowStates);
        }

        #endregion

        #region GetWorkflowAvailableProjectsAsync
        [TestMethod]
         public async Task GetWorkflowAvailableProjectsAsync_ExistsAvailableProjects_QueryReturnAvailableProjects()
         {
            //arrange    
            int workflowId = 1;
            int folderId = 2;            

            var listAvailableProjects = new List<InstanceItem>
             {
                 new InstanceItem
                 {
                     Id=4,
                     ParentFolderId=2,
                     Name="Project11",
                     Type=(InstanceItemTypeEnum)1
                 },
                 new InstanceItem
                 {
                     Id=7,
                     ParentFolderId=2,
                     Name="Project12",
                     Type=(InstanceItemTypeEnum)1
                 }
             };

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), listAvailableProjects);
 
             //act
             var result = await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
 
             //assert
             Assert.IsNotNull(result);
         }
 
         [TestMethod]
         public async Task GetWorkflowAvailableProjects_InvalidWorkflowId_ReturnArgumentOutOfRangeException()
         {
            //arrange    
            int workflowId = 0;
            int folderId = 2;
            Exception exception = null;

            //act
            try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                exception = ex;
             }
 
             //assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ArgumentOutOfRangeException));
         }
 
         [TestMethod]
         public async Task GetWorkflowAvailableProjects_InvalidFolderId_ReturnArgumentOutOfRangeException()
         {
            //arrange    
            int workflowId = 1;
            int folderId = 0;
            Exception exception = null;

            //act
            try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                 exception = ex;
             }
 
             //assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ArgumentOutOfRangeException));
         }
 
         [TestMethod]
         public async Task GetWorkflowAvailableProjects_NotExistFolderByFolderId_ReturnResourceNotFoundException()
         {
            //arrange                          
            int workflowId = 1;
            int folderId = 99999;
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), new List<InstanceItem>(), new Dictionary<string, object> { { "ErrorCode" , (int)SqlErrorCodes.FolderWithCurrentIdNotExist} });
 
             //act
             try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                exception = ex;
             }
 
             //assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
         }
 
         [TestMethod]
         public async Task GetWorkflowAvailableProjects_NotExistWorkflowByWorkflowId_ReturnResourceNotFoundException()
         {
            //arrange      
            int workflowId = 99999;
            int folderId = 1;
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), new List<InstanceItem>(), new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist } });
 
             //act
             try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                 exception = ex;
             }
             //assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
         }
        #endregion

        #region GetProjectArtifactsAssignedToWorkflowAsync

        [TestMethod]
        public async Task GetProjectArtifactsAssignedToWorkflowAsync_ArtifactsAreFound_SuccessfulResult()
        {
            // Arrange
            int errorCode = 0;
            int total = 1;

            var spResult = new List<WorkflowProjectArtifacts>
            {
                new WorkflowProjectArtifacts()
                {
                    ProjectId = 1,
                    ProjectName = "TestProject",
                    ArtifactId = 1,
                    ArtifactName = "TestArtifact"
                }
            };

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowProjectsArtifactTypes",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        spResult,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode },
                                                            { "Total", total }
                                                        });
            // Act
            var artifacts = await
                _workflowRepository.GetProjectArtifactsAssignedtoWorkflowAsync(WorkflowId, Pagination);

            // Assert
            Assert.IsNotNull(artifacts);
            Assert.AreEqual(total, artifacts.Total);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectArtifactsAssignedToWorkflowAsync_InvalidWorkflowId_ReturnResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            WorkflowId = 1000;
            int total = 0;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowProjectsArtifactTypes",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        new List<WorkflowProjectArtifacts>(),
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode },
                                                            { "Total", total }
                                                        });
            // Act
            await
                _workflowRepository.GetProjectArtifactsAssignedtoWorkflowAsync(WorkflowId, Pagination);

            // Assert
            _sqlConnectionWrapperMock.Verify();

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectArtifactsAssignedToWorkflowAsync_WorkflowIdNotValid_ThrowsArgumentOutOfRangeException()
        {
            //Arrange
            WorkflowId = 0;

            // Act
            await
                _workflowRepository.GetProjectArtifactsAssignedtoWorkflowAsync(WorkflowId, Pagination);

            // Assert
            _sqlConnectionWrapperMock.Verify();
        }

        #endregion
    }
}
