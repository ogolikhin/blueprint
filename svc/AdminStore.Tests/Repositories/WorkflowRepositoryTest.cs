using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

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
            _workflowAssignScope = new WorkflowAssignScope() { AllArtifacts = true, AllProjects = true, ArtifactIds = new List<int>() { 145, 148 }, ProjectIds = new List<int>() { 1, 4 } };
            _projectsUnassignedScope = new OperationScope() { Ids = new List<int>() { 1, 2 }, SelectAll = false };

            _listArtifactTypesIds = new List<int> { 1, 2, 3 };
            _workflowId = 1;
            _pagination = new Pagination() { Limit = int.MaxValue, Offset = 0 };

            _scope = new OperationScope() { Ids = _listArtifactTypesIds, SelectAll = false };
        }

        private SqlConnectionWrapperMock _sqlConnectionWrapperMock;
        private WorkflowRepository _workflowRepository;
        private Mock<ISqlHelper> _sqlHelperMock;
        private WorkflowAssignScope _workflowAssignScope;
        private OperationScope _projectsUnassignedScope;
        private int _workflowId;
        private string _workflowName = "TestWorkflowName";
        private const int _projectId = 1;
        private Pagination _pagination;
        private List<int> _listArtifactTypesIds;
        private IEnumerable<SyncResult> _outputSyncResult = new List<SyncResult>() { new SyncResult { TotalAdded = 2, TotalDeleted = 1 } };
        private string _projectSearch = "test";
        private OperationScope _scope;
        private int _userId = 1;

        #region AssignProjectsAndArtifactTypesToWorkflow
        [TestMethod]
        public async Task AssignProjectsAndArtifactTypesToWorkflow_ExistsAssignedProjects_QueryReturnCountAssignedProjects()
        {
            // arrange
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactTypesToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "AllProjectsAssignedToWorkflow", true } });

            // act
            var result = await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(_workflowId, _workflowAssignScope);

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactTypesToWorkflow_WorkflowByWorkflowIdIsActive_ReturnConflictExceptionE()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactTypesToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive } });
            // act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactTypesToWorkflow_WorkflowByWorkflowIdIsNotFound_ReturnResourceNotFoundExceptionE()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactTypesToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist } });
            // act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactTypesToWorkflow_GeneralSQLException_ReturnException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("AssignProjectsAndArtifactTypesToWorkflow", It.IsAny<Dictionary<string, object>>(), 2, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.GeneralSqlError } });
            // act
            try
            {
                await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(_workflowId, _workflowAssignScope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }
        #endregion

        #region AssignArtifactTypesToProjectInWorkflow
        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_ExistsAssignedProjects_QueryReturnCountAddedAndDeletedProjects()
        {
            // arrange
            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult);

            // act
            var result = await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_WorkflowWithCurrentIdIsActive_ReturnConflictException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdIsActive } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (ConflictException ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_WorkflowWithCurrentIdNotExist_ReturnResourceNotFoundException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_GeneralSQLException_ReturnException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.GeneralSqlError } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(Exception));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_ProjectOfWorkflowDoesNotHaveArtifactTypes_ReturnConflictException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowProjectHasNoArtifactTypes } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ConflictException));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_ProjectWithCurrentIdNotExist_ReturnResourceNotFoundException()
        {
            // arrange
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("AssignArtifactTypesToProjectInWorkflow", It.IsAny<Dictionary<string, object>>(), _outputSyncResult, new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.ProjectWithCurrentIdNotExist } });
            // act
            try
            {
                await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(_workflowId, _projectId, _scope);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }
            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
        }
        #endregion

        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflowDetailsAsync_WeHaveThisWorkflowInDb_QueryReturnWorkflow()
        {
            // arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);

            var workflowId = 10;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description" };
            var workflowsList = new List<SqlWorkflow> { workflow };
            cxn.SetupQueryAsync("GetWorkflowDetails", new Dictionary<string, object> { { "WorkflowId", workflowId } }, workflowsList);

            // act
            var workflowDetails = await repository.GetWorkflowDetailsAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region GetWorkflowArtifactTypesAndProjectsAsync

        [TestMethod]
        public async Task GetWorkflowArtifactTypesAndProjectsAsync_ThereExistWorkflowArtifactTypesAndProjects_QueryReturnWorkflowArtifactTypesAndProjects()
        {
            // arrange
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

            // act
            var workflowDetails = await repository.GetWorkflowArtifactTypesAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region UpdateWorkflowsAsync

        [TestMethod]
        public async Task UpdateWorkflowsAsync_UpdateThisWorkflowInDb_QueryReturnWorkflows()
        {
            // arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);
            var publishRevision = 12;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description", Active = true };
            var workflowsList = new List<SqlWorkflow> { workflow };
            cxn.SetupQueryAsync("UpdateWorkflows", It.IsAny<Dictionary<string, object>>(), workflowsList);

            // act
            var updatedWorkflows = await repository.UpdateWorkflowsAsync(workflowsList, publishRevision);

            // assert
            Assert.IsNotNull(updatedWorkflows);
        }

        #endregion

        #region GetWorkflowTransitionsAndPropertyChangesByWorkflowId

        [TestMethod]
        public async Task GetWorkflowTransitionsAndPropertyChangesByWorkflowId_ThereExistWorkflowTransitionsAndPropertyChanges_QueryReturnWorkflowTransitionsAndPropertyChanges()
        {
            // arrange
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

            // act
            var workflowDetails = await repository.GetWorkflowEventsAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowDetails);
        }

        #endregion

        #region GetWorkflowStatesByWorkflowId

        [TestMethod]
        public async Task GetWorkflowStatesByWorkflowId_WeHaveThisWorkflowInDb_QueryReturnWorkflowStates()
        {
            // arrange
            var cxn = new SqlConnectionWrapperMock();
            var sqlHelperMock = new Mock<ISqlHelper>();

            var repository = new WorkflowRepository(cxn.Object, sqlHelperMock.Object);

            var workflowId = 10;
            var workflow = new SqlState { Name = "Workflow1", Default = true };
            var workflowsList = new List<SqlState> { workflow };
            cxn.SetupQueryAsync("GetWorkflowStatesById", new Dictionary<string, object> { { "WorkflowId", workflowId } }, workflowsList);

            // act
            var workflowStates = await repository.GetWorkflowStatesAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowStates);
        }

        #endregion

        #region GetWorkflowAvailableProjectsAsync
        [TestMethod]
         public async Task GetWorkflowAvailableProjectsAsync_ExistsAvailableProjects_QueryReturnAvailableProjects()
         {
            // arrange
            int workflowId = 1;
            int folderId = 2;

            var listAvailableProjects = new List<InstanceItem>
             {
                 new InstanceItem
                 {
                     Id = 4,
                     ParentFolderId = 2,
                     Name = "Project11",
                     Type = (InstanceItemTypeEnum)1
                 },
                 new InstanceItem
                 {
                     Id = 7,
                     ParentFolderId = 2,
                     Name = "Project12",
                     Type = (InstanceItemTypeEnum)1
                 }
             };

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), listAvailableProjects);

             // act
             var result = await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);

             // assert
             Assert.IsNotNull(result);
         }

         [TestMethod]
         public async Task GetWorkflowAvailableProjects_InvalidWorkflowId_ReturnArgumentOutOfRangeException()
         {
            // arrange
            int workflowId = 0;
            int folderId = 2;
            Exception exception = null;

            // act
            try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                exception = ex;
             }

             // assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ArgumentOutOfRangeException));
         }

         [TestMethod]
         public async Task GetWorkflowAvailableProjects_InvalidFolderId_ReturnArgumentOutOfRangeException()
         {
            // arrange
            int workflowId = 1;
            int folderId = 0;
            Exception exception = null;

            // act
            try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                 exception = ex;
             }

             // assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ArgumentOutOfRangeException));
         }

         [TestMethod]
         public async Task GetWorkflowAvailableProjects_NotExistFolderByFolderId_ReturnResourceNotFoundException()
         {
            // arrange
            int workflowId = 1;
            int folderId = 99999;
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), new List<InstanceItem>(), new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.FolderWithCurrentIdNotExist } });

             // act
             try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                exception = ex;
             }

             // assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
         }

         [TestMethod]
         public async Task GetWorkflowAvailableProjects_NotExistWorkflowByWorkflowId_ReturnResourceNotFoundException()
         {
            // arrange
            int workflowId = 99999;
            int folderId = 1;
            Exception exception = null;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowAvailableProjects", It.IsAny<Dictionary<string, object>>(), new List<InstanceItem>(), new Dictionary<string, object> { { "ErrorCode", (int)SqlErrorCodes.WorkflowWithCurrentIdNotExist } });

             // act
             try
             {
                 await _workflowRepository.GetWorkflowAvailableProjectsAsync(workflowId, folderId);
             }
             catch (Exception ex)
             {
                 exception = ex;
             }
             // assert
             Assert.IsNotNull(exception);
             Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
         }
        #endregion

        #region UnassignProjectsAndArtifactTypesFromWorkflowAsync

        [TestMethod]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_ProjectsUnassigned_SuccessfulResult()
        {
            // Arrange
            var unassignedCount = 2;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("UnassignProjectsAndArtifactTypesFromWorkflow", It.IsAny<Dictionary<string, object>>(), unassignedCount);

            // act
            var result = await _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(_workflowId, _projectsUnassignedScope);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(unassignedCount, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_WorkflowIdNotExist_ReturnResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            int total = 0;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("UnassignProjectsAndArtifactTypesFromWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        total,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(_workflowId, _projectsUnassignedScope);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_WorkflowIsActive_ReturnConflictException()
        {
            // Arrange
            int errorCode = 50025;
            int total = 0;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("UnassignProjectsAndArtifactTypesFromWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        total,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(_workflowId, _projectsUnassignedScope);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_WorkflowIsActive_ReturnException()
        {
            // Arrange
            int errorCode = 50000;
            int total = 0;

            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("UnassignProjectsAndArtifactTypesFromWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        total,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(_workflowId, _projectsUnassignedScope);

            // Assert

        }

        #endregion

        #region GetProjectArtifactTypesAssignedToWorkflowAsync

        [TestMethod]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_ArtifactsAreFound_SuccessfulResult()
        {
            // Arrange
            int errorCode = 0;
            int total = 1;

            var spResult = new List<WorkflowProjectArtifactType>
            {
                new WorkflowProjectArtifactType()
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
                _workflowRepository.GetProjectArtifactTypesAssignedtoWorkflowAsync(_workflowId, _pagination);

            // Assert
            Assert.IsNotNull(artifacts);
            Assert.AreEqual(total, artifacts.Total);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_InvalidWorkflowId_ReturnResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            _workflowId = 1000;
            int total = 0;

            _sqlConnectionWrapperMock.SetupQueryAsync("GetWorkflowProjectsArtifactTypes",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        new List<WorkflowProjectArtifactType>(),
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode },
                                                            { "Total", total }
                                                        });
            // Act
            await
                _workflowRepository.GetProjectArtifactTypesAssignedtoWorkflowAsync(_workflowId, _pagination);

            // Assert
            _sqlConnectionWrapperMock.Verify();

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_WorkflowIdNotValid_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            _workflowId = 0;

            // Act
            await
                _workflowRepository.GetProjectArtifactTypesAssignedtoWorkflowAsync(_workflowId, _pagination);

            // Assert
            _sqlConnectionWrapperMock.Verify();
        }

        #endregion

        #region SearchProjectsByName

        [TestMethod]
        public async Task SearchProjectsByName_AllParamsAreCorrect_ReturnProjects()
        {
            // Arrange
            int errorCode = 0;

            var expectedProjects = new List<WorkflowProjectSearch>()
            {
                new WorkflowProjectSearch()
                {
                    ItemId = 1,
                    Name = "Project1",
                    Path = "Path1"
                },
                new WorkflowProjectSearch()
                {
                    ItemId = 2,
                    Name = "Project2",
                    Path = "Path2"
                }
            };

            _sqlConnectionWrapperMock.SetupQueryAsync("SearchProjectsByName",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        expectedProjects,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            var actualProjects = await
                _workflowRepository.SearchProjectsByName(_workflowId, _projectSearch);

            // Assert
            Assert.IsNotNull(actualProjects);
            Assert.AreEqual(expectedProjects.Count, actualProjects.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task SearchProjectsByName_InvalidWorkflowId_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            _workflowId = 0;

            // Act
            await
                _workflowRepository.SearchProjectsByName(_workflowId, _projectSearch);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task SearchProjectsByName_InvalidWorkflowId_ReturnResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            _workflowId = 1000;

            _sqlConnectionWrapperMock.SetupQueryAsync("SearchProjectsByName",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        new List<WorkflowProjectSearch>(),
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await _workflowRepository.SearchProjectsByName(_workflowId, _projectSearch);

            // Assert
        }

        #endregion

        #region CopyWorkflowAsync

        [TestMethod]
        public async Task CopyWorkflowAsync_AllParamsAreCorrect_UpdateWorkflowSuccessfuly()
        {
            // Arrange
            int errorCode = 0;
            var updatedWorkflowId = 2;
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("CopyWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        updatedWorkflowId,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            var copyWorkflowResult = await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _workflowName);

            // Assert
            Assert.IsNotNull(copyWorkflowResult);
            Assert.AreEqual(updatedWorkflowId, copyWorkflowResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task CopyWorkflowAsync_WorkflowIdNotValid_ArgumentOutOfRangeException()
        {
            // Arrange
            _workflowId = 0;

            // Act
            await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _workflowName);

            // Assert

        }
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task CopyWorkflowAsync_WorkflowIdNotExists_ResourceNotFoundException()
        {
            // Arrange
            int errorCode = 50024;
            var updatedWorkflowId = 0;
            _workflowId = 10000;
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("CopyWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        updatedWorkflowId,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _workflowName);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task CopyWorkflowAsync_WorkflowWithSpecifiedNameAlreadyExists_ConflictException()
        {
            // Arrange
            int errorCode = 50023;
            var updatedWorkflowId = 0;
            _workflowId = 10000;
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("CopyWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        updatedWorkflowId,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _workflowName);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CopyWorkflowAsync_GeneralError_Exception()
        {
            // Arrange
            int errorCode = 50000;
            var updatedWorkflowId = 0;
            _sqlConnectionWrapperMock.SetupExecuteScalarAsync("CopyWorkflow",
                                                        It.IsAny<Dictionary<string, object>>(),
                                                        updatedWorkflowId,
                                                        new Dictionary<string, object>
                                                        {
                                                            { "ErrorCode", errorCode }
                                                        });
            // Act
            await
                _workflowRepository.CopyWorkflowAsync(_workflowId, _userId, _workflowName);

            // Assert
        }

        #endregion
    }
}
